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
using System.IO;

using BilliardManagement.Common.Responses;

namespace BilliardManagement.Business.Services
{
    public class ProductService : IProductService
    {
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

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _unitOfWork.Repository<Product>().GetFirstOrDefaultAsync(
                filter: p => p.Id == id && !p.IsDeleted,
                includeProperties: "Category"
            );

            if (product == null)
                throw new CustomException("Sản phẩm không tồn tại", 404);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new CustomException("Tên sản phẩm không được để trống", 400);

            if (dto.Price <= 0)
                throw new CustomException("Giá bán phải lớn hơn 0", 400);

            var isExists = await _unitOfWork.Repository<Product>()
                .AnyAsync(x => x.ProductName.Trim().ToLower() == dto.Name.Trim().ToLower() && !x.IsDeleted);
            if (isExists)
            {
                _logger.LogWarning("Duplicate product name creation attempt: {ProductName} by {CreatedBy} at {Time}", dto.Name, createdBy, DateTime.UtcNow);
                throw new CustomException("Tên sản phẩm đã tồn tại", 400);
            }

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);
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

            var isExists = await _unitOfWork.Repository<Product>()
                .AnyAsync(x => x.Id != id && x.ProductName.Trim().ToLower() == dto.Name.Trim().ToLower() && !x.IsDeleted);
            if (isExists)
            {
                _logger.LogWarning("Duplicate product name update attempt: {ProductName} (ID: {ProductId}) by {UpdatedBy} at {Time}", dto.Name, id, updatedBy, DateTime.UtcNow);
                throw new CustomException("Tên sản phẩm đã tồn tại", 400);
            }

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);
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

        public async Task<bool> CheckDuplicateNameAsync(string name, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            
            if (excludeId.HasValue)
            {
                return await _unitOfWork.Repository<Product>().AnyAsync(x => x.Id != excludeId.Value && x.ProductName.Trim().ToLower() == name.Trim().ToLower() && !x.IsDeleted);
            }
            return await _unitOfWork.Repository<Product>().AnyAsync(x => x.ProductName.Trim().ToLower() == name.Trim().ToLower() && !x.IsDeleted);
        }

        public async Task<PagedResult<ProductDto>> GetPagedProductsAsync(ProductQueryParameters query)
        {
            var filters = new List<System.Linq.Expressions.Expression<System.Func<Product, bool>>>();
            filters.Add(p => !p.IsDeleted);

            if (query.CategoryId.HasValue)
            {
                filters.Add(p => p.CategoryId == query.CategoryId.Value);
            }
            if (query.IsAvailable.HasValue)
            {
                filters.Add(p => p.IsAvailable == query.IsAvailable.Value);
            }
            if (query.MinPrice.HasValue)
            {
                filters.Add(p => p.Price >= query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                filters.Add(p => p.Price <= query.MaxPrice.Value);
            }
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                filters.Add(p => p.ProductName.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("Price", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price);
                }
                else if (query.SortBy.Equals("StockQuantity", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(p => p.StockQuantity) : q.OrderBy(p => p.StockQuantity);
                }
                else if (query.SortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(p => p.ProductName) : q.OrderBy(p => p.ProductName);
                }
            }
            else
            {
                orderBy = q => q.OrderBy(p => p.ProductName);
            }

            var (items, totalCount) = await _unitOfWork.Repository<Product>().GetPagedAsync(
                filters: filters,
                orderBy: orderBy,
                includeProperties: "Category",
                page: query.PageNumber,
                pageSize: query.PageSize
            );

            var mappedItems = _mapper.Map<IEnumerable<ProductDto>>(items);
            return new PagedResult<ProductDto>(mappedItems, query.PageNumber, query.PageSize, totalCount);
        }
    }
}
