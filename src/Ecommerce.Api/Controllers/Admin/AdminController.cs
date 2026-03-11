using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Identity;
using Ecommerce.Application.Interfaces.Catalog;
using Ecommerce.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Ecommerce.Api.Controllers.Admin
{
    /// <summary>
    /// Admin operations — split into user management and category management endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ICategoryService _categoryService;

        public AdminController(IUserManagementService userManagementService, ICategoryService categoryService)
        {
            _userManagementService = userManagementService;
            _categoryService = categoryService;
        }

        // === User Management ===

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Ok(await _userManagementService.GetAllUsersAsync(pageNumber, pageSize));

        [HttpPatch("users/block-unblock/{userId}")]
        public async Task<IActionResult> ToggleUserBlockStatus(Guid userId)
        {
            var isBlocked = await _userManagementService.ToggleUserBlockStatusAsync(userId);
            return Ok(new { message = isBlocked ? "User blocked" : "User unblocked" });
        }

        // === Category Management ===

        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryRequestDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Category data is required." });
            var message = await _categoryService.CreateCategoryAsync(dto);
            return Ok(new { message });
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
            => Ok(await _categoryService.GetAllCategoriesAsync());
    }
}
