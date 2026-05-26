using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace BilliardManagement.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) throw new CustomException("Không tìm thấy danh mục", 404);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                throw new CustomException("Tên danh mục không được để trống", 400);

            var isExists = await _unitOfWork.Repository<Category>()
                .AnyAsync(x => x.CategoryName.Trim().ToLower() == dto.CategoryName.Trim().ToLower());
            if (isExists)
                throw new CustomException("Tên danh mục đã tồn tại", 400);

            var category = _mapper.Map<Category>(dto);
            category.CategoryName = dto.CategoryName.Trim();
            category.Description = dto.Description?.Trim();

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category created: id={CategoryId}, name={CategoryName}", category.Id, category.CategoryName);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                throw new CustomException("Tên danh mục không được để trống", 400);

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) throw new CustomException("Không tìm thấy danh mục", 404);

            var isExists = await _unitOfWork.Repository<Category>()
                .AnyAsync(x => x.Id != id && x.CategoryName.Trim().ToLower() == dto.CategoryName.Trim().ToLower());
            if (isExists)
                throw new CustomException("Tên danh mục đã tồn tại", 400);

            category.CategoryName = dto.CategoryName.Trim();
            category.Description = dto.Description?.Trim();

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category updated: id={CategoryId}, name={CategoryName}", category.Id, category.CategoryName);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) throw new CustomException("Không tìm thấy danh mục", 404);

            // Check if there are active or soft-deleted products under this category
            var hasProducts = await _unitOfWork.Repository<Product>().AnyAsync(p => p.CategoryId == id && !p.IsDeleted);
            if (hasProducts)
                throw new CustomException("Không thể xóa danh mục vì đang có sản phẩm thuộc danh mục này.", 400);

            _unitOfWork.Repository<Category>().Remove(category);
            var result = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category deleted: id={CategoryId}", id);

            return result > 0;
        }
    }
}
