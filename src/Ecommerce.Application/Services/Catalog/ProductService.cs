using AutoMapper;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Interfaces.Catalog;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.Application.Services.Catalog
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudImageService _cloudImageService;
        private readonly IMemoryCache _cache;
        private const string ProductsCacheKey = "products_cache";

        public ProductService(
            IRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICloudImageService cloudImageService,
            IMemoryCache cache)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudImageService = cloudImageService;
            _cache = cache;
        }

        public async Task AddProductAsync(CreateProductRequestDto productDto, IFormFile image)
        {
            string imageUrl = await _cloudImageService.UploadImageAsync(image);
            var product = _mapper.Map<Product>(productDto);
            product.Image = imageUrl;
            await _productRepo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(ProductsCacheKey);
        }

        public async Task<bool> UpdateProductAsync(Guid id, CreateProductRequestDto productDto, IFormFile image)
        {
            var product = await _productRepo.Query().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            string imageUrl = await _cloudImageService.UploadImageAsync(image);
            product.Image = imageUrl;
            _mapper.Map(productDto, product);
            _productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(ProductsCacheKey);
            return true;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;
            _productRepo.Remove(product);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(ProductsCacheKey);
            return true;
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(Guid productId)
        {
            var product = await _productRepo.Query()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found");

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<PagedResult<ProductResponseDto>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _productRepo.Query().AsNoTracking().Include(p => p.Category);
            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            return new PagedResult<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(products),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
