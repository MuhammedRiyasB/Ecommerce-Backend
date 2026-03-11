using Ecommerce.Application.DTOs.Identity;

namespace Ecommerce.Application.Interfaces.Identity
{
    /// <summary>
    /// Handles user authentication operations: registration, login, and token management.
    /// </summary>
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterRequestDto registerDto, string role = "User");
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
