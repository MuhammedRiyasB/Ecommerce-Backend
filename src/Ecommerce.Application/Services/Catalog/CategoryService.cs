using AutoMapper;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.Interfaces.Catalog;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services.Catalog
{
    /// <summary>
    /// ISP fix: handles only category management (was previously combined with user management in AdminService).
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> categoryRepo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<string> CreateCategoryAsync(CreateCategoryRequestDto createCategory)
        {
            if (createCategory == null || string.IsNullOrWhiteSpace(createCategory.CategoryName))
                throw new ArgumentException("Invalid category data.");

            bool exists = await _categoryRepo.Query()
                .AnyAsync(c => c.CategoryName.ToLower() == createCategory.CategoryName.ToLower());
            if (exists)
                throw new ArgumentException("Category already exists.");

            var category = _mapper.Map<Category>(createCategory);
            await _categoryRepo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return "Category added successfully.";
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
        }
    }
}
