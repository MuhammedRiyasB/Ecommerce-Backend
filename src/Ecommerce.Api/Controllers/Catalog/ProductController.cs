using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Interfaces.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Ecommerce.Api.Controllers.Catalog
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService) => _productService = productService;

        [HttpPost("Add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromForm] CreateProductRequestDto productDto, IFormFile image)
        {
            if (productDto == null) return BadRequest(new { message = "Product details cannot be null" });
            await _productService.AddProductAsync(productDto, image);
            return Ok(new { message = "Product added successfully" });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product == null ? NotFound(new { message = $"Product with ID {id} not found" }) : Ok(product);
        }

        [HttpGet("All")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return Ok(await _productService.GetAllProductsAsync(pageNumber, pageSize));
        }

        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] CreateProductRequestDto productDto, IFormFile image)
        {
            if (productDto == null) return BadRequest(new { message = "Product details cannot be null" });
            var updated = await _productService.UpdateProductAsync(id, productDto, image);
            return updated ? Ok(new { message = "Product updated successfully" }) : NotFound(new { message = $"Product with ID {id} not found" });
        }

        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            return deleted ? Ok(new { message = "Product deleted successfully" }) : NotFound(new { message = $"Product with ID {id} not found" });
        }
    }
}
