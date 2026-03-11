using AutoMapper;
using Ecommerce.Application.DTOs.Identity;
using Ecommerce.Application.Interfaces.Identity;
using Ecommerce.Application.Common.Settings;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Application.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<RefreshToken> _refreshTokenRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IRepository<User> userRepo,
            IRepository<RefreshToken> refreshTokenRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto registerDto, string role = "User")
        {
            if (await _userRepo.Query().AnyAsync(u => u.Email == registerDto.Email.ToLower()))
                throw new ArgumentException("Email already exists");

            if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
                throw new ArgumentException($"Invalid role: {role}");

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            user.UserId = Guid.NewGuid();
            user.Role = parsedRole;

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _userRepo.Query().FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());
            if (user == null)
                throw new ArgumentException("Invalid Email");

            if (user.IsBlocked)
                throw new UnauthorizedAccessException("Your account has been blocked. Please contact support.");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new ArgumentException("Invalid Password");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepo.Query()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
                throw new UnauthorizedAccessException("Invalid refresh token");

            // Reuse detection: if token was already revoked, an attacker is replaying a stolen token
            if (storedToken.IsRevoked)
            {
                await RevokeTokenFamilyAsync(storedToken.TokenFamily);
                throw new UnauthorizedAccessException(
                    "Refresh token reuse detected. All sessions for this token family have been revoked for security.");
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired");

            storedToken.IsRevoked = true;
            _refreshTokenRepo.Update(storedToken);
            await _unitOfWork.SaveChangesAsync();

            return await GenerateAuthResponseAsync(storedToken.User, storedToken.TokenFamily);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepo.Query()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (storedToken != null)
            {
                await RevokeTokenFamilyAsync(storedToken.TokenFamily);
            }
        }

        private async Task RevokeTokenFamilyAsync(Guid tokenFamily)
        {
            var familyTokens = await _refreshTokenRepo.Query()
                .Where(rt => rt.TokenFamily == tokenFamily && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in familyTokens)
            {
                token.IsRevoked = true;
                _refreshTokenRepo.Update(token);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, Guid? existingFamily = null)
        {
            var tokenFamily = existingFamily ?? Guid.NewGuid();
            var accessToken = CreateAccessToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.UserId, tokenFamily);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };
        }

        private string CreateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(30)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> CreateRefreshTokenAsync(Guid userId, Guid tokenFamily)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var tokenString = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                TokenFamily = tokenFamily
            };

            await _refreshTokenRepo.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();
            return tokenString;
        }
    }
}
