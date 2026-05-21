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
    public class ProductService : IProductService
    {
        private static readonly string[] AllowedCategories =
            { "Nước ngọt", "Cafe", "Bia", "Snack", "Trà sữa" };

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync(p => !p.IsDeleted, "Category");
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new CustomException("Tên sản phẩm không được để trống", 400);

            if (dto.Price <= 0)
                throw new CustomException("Giá bán phải lớn hơn 0", 400);

            if (!AllowedCategories.Contains(dto.Category?.Trim() ?? "", StringComparer.OrdinalIgnoreCase))
                throw new CustomException("Danh mục sản phẩm không hợp lệ", 400);

            var category = await _unitOfWork.Repository<Category>()
                .GetFirstOrDefaultAsync(c => c.CategoryName == dto.Category.Trim());
            if (category == null)
                throw new CustomException("Danh mục không tồn tại trong hệ thống", 400);

            var product = _mapper.Map<Product>(dto);
            product.ProductName = dto.Name.Trim();
            product.CategoryId = category.Id;
            product.Description = dto.Description?.Trim();
            product.IsAvailable = dto.IsAvailable;
            product.ImageUrl = dto.ImageUrl;

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            product.Category = category;

            _logger.LogInformation(
                "Product created: productId={ProductId}, name={ProductName}, category={Category}, createdBy={CreatedBy}",
                product.Id, product.ProductName, category.CategoryName, createdBy);

            return _mapper.Map<ProductDto>(product);
        }

        public static bool IsValidImageExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && AllowedImageExtensions.Contains(ext);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, CreateProductDto dto, Guid? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new CustomException("Tên sản phẩm không được để trống", 400);

            if (dto.Price <= 0)
                throw new CustomException("Giá bán phải lớn hơn 0", 400);

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) throw new CustomException("Product not found", 404);

            var category = await _unitOfWork.Repository<Category>()
                .GetFirstOrDefaultAsync(c => c.CategoryName == dto.Category.Trim());
            if (category == null)
                throw new CustomException("Danh mục không tồn tại trong hệ thống", 400);

            product.ProductName = dto.Name.Trim();
            product.CategoryId = category.Id;
            product.Price = dto.Price;
            product.StockQuantity = dto.Stock;
            product.IsAvailable = dto.IsAvailable;
            if (dto.ImageUrl != null)
            {
                product.ImageUrl = dto.ImageUrl;
            }

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            product.Category = category;

            _logger.LogInformation(
                "Product updated: productId={ProductId}, name={ProductName}, updatedBy={UpdatedBy}",
                id, product.ProductName, updatedBy);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null || product.IsDeleted) return false;

            var hasOrders = await _unitOfWork.Repository<OrderItem>().AnyAsync(oi => oi.ProductId == id);
            if (hasOrders)
            {
                product.IsDeleted = true;
                product.IsAvailable = false;
                _unitOfWork.Repository<Product>().Update(product);
            }
            else
            {
                _unitOfWork.Repository<Product>().Remove(product);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product deleted: productId={ProductId}, softDelete={IsSoftDelete}", id, hasOrders);
            return result > 0;
        }
    }
}
