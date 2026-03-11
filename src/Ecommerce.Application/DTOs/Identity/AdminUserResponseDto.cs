namespace Ecommerce.Application.DTOs.Identity
{
    public class AdminUserResponseDto : UserResponseDto
    {
        public string Role { get; set; } = null!;
    }
}
