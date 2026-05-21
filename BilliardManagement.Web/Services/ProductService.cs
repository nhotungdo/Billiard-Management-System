using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

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
            return await GetAsync<List<ProductDto>>("products");
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            return await GetAsync<ProductDto>($"products/{id}");
        }

        public async Task<ProductDto?> CreateProductAsync(CreateProductDto request)
        {
            return await PostAsync<CreateProductDto, ProductDto>("products", request);
        }

        public async Task<bool> UpdateProductAsync(Guid id, CreateProductDto request)
        {
            return await PutAsync($"products/{id}", request);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            return await DeleteAsync($"products/{id}");
        }
    }
}
