using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Interfaces.Catalog
{
    /// <summary>
    /// Manages products — CRUD operations with image upload support.
    /// </summary>
    public interface IProductService
    {
        Task AddProductAsync(CreateProductRequestDto productDto, IFormFile image);
        Task<bool> UpdateProductAsync(Guid productId, CreateProductRequestDto productDto, IFormFile image);
        Task<bool> DeleteProductAsync(Guid id);
        Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ProductResponseDto> GetProductByIdAsync(Guid id);
    }
}
