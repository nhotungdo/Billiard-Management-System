using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Services
{
    public class ProductService : BaseApiService
    {
        public ProductService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<ProductDto>?> GetAllProductsAsync()
        {
            var result = await GetAsync<PagedResult<ProductDto>>("products?pageSize=1000");
            return result?.Items;
        }

        public async Task<PagedResult<ProductDto>?> GetPagedProductsAsync(int pageNumber, int pageSize, Guid? categoryId = null, bool? isAvailable = null, decimal? minPrice = null, decimal? maxPrice = null, string? searchTerm = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"products?pageNumber={pageNumber}&pageSize={pageSize}";
            if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
            if (isAvailable.HasValue) url += $"&isAvailable={isAvailable.Value}";
            if (minPrice.HasValue) url += $"&minPrice={minPrice.Value}";
            if (maxPrice.HasValue) url += $"&maxPrice={maxPrice.Value}";
            if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<ProductDto>>(url);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            return await GetAsync<ProductDto>($"products/{id}");
        }

        public async Task<ProductDto?> CreateProductAsync(CreateProductDto request)
        {
            return await PostAsync<CreateProductDto, ProductDto>("products", request);
        }

        public async Task<ProductDto?> CreateProductWithImageAsync(
            string name,
            Guid categoryId,
            decimal price,
            string? description,
            bool isAvailable,
            int stock,
            IFormFile? image)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "name");
            form.Add(new StringContent(categoryId.ToString()), "categoryId");
            form.Add(new StringContent(price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "price");
            form.Add(new StringContent(description ?? ""), "description");
            form.Add(new StringContent(isAvailable.ToString().ToLowerInvariant()), "isAvailable");
            form.Add(new StringContent(stock.ToString()), "stock");

            if (image != null && image.Length > 0)
            {
                var streamContent = new StreamContent(image.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                form.Add(streamContent, "image", image.FileName);
            }

            return await PostMultipartAsync<ProductDto>("products", form);
        }

        public async Task<bool> UpdateProductAsync(Guid id, CreateProductDto request)
        {
            return await PutAsync($"products/{id}", request);
        }

        public async Task<bool> UpdateProductWithImageAsync(
            Guid id,
            string name,
            Guid categoryId,
            decimal price,
            string? description,
            bool isAvailable,
            int stock,
            string? existingImageUrl,
            IFormFile? image)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "name");
            form.Add(new StringContent(categoryId.ToString()), "categoryId");
            form.Add(new StringContent(price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "price");
            form.Add(new StringContent(description ?? ""), "description");
            form.Add(new StringContent(isAvailable.ToString().ToLowerInvariant()), "isAvailable");
            form.Add(new StringContent(stock.ToString()), "stock");
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                form.Add(new StringContent(existingImageUrl), "existingImageUrl");
            }

            if (image != null && image.Length > 0)
            {
                var streamContent = new StreamContent(image.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                form.Add(streamContent, "image", image.FileName);
            }

            return await PutMultipartAsync($"products/{id}", form);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            return await DeleteAsync($"products/{id}");
        }

        // --- Category Management APIs ---
        
        public async Task<List<CategoryDto>?> GetAllCategoriesAsync()
        {
            return await GetAsync<List<CategoryDto>>("categories");
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            return await GetAsync<CategoryDto>($"categories/{id}");
        }

        public async Task<CategoryDto?> CreateCategoryAsync(CreateCategoryDto request)
        {
            return await PostAsync<CreateCategoryDto, CategoryDto>("categories", request);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto request)
        {
            return await PutAsync<CreateCategoryDto, CategoryDto>($"categories/{id}", request);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            return await DeleteAsync($"categories/{id}");
        }
    }
}
