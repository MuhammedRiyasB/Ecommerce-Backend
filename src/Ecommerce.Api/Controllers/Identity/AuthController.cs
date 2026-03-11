using Ecommerce.Application.DTOs.Identity;
using Ecommerce.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Ecommerce.Api.Controllers.Identity
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid registration data." });
            var user = await _authService.RegisterAsync(dto);
            return Ok(new { message = "Registration successful", data = user });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid login data." });
            var auth = await _authService.LoginAsync(dto);
            return Ok(auth);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Refresh token is required." });
            var auth = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(auth);
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Refresh token is required." });
            await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);
            return Ok(new { message = "Token revoked successfully." });
        }
    }
}
