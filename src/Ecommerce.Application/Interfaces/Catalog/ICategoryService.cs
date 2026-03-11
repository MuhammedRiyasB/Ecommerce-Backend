using Ecommerce.Application.DTOs.Category;

namespace Ecommerce.Application.Interfaces.Catalog
{
    /// <summary>
    /// Manages product categories — CRUD operations.
    /// Split from the old IAdminService (ISP: separate category management from user management).
    /// </summary>
    public interface ICategoryService
    {
        Task<string> CreateCategoryAsync(CreateCategoryRequestDto createCategory);
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
    }
}
