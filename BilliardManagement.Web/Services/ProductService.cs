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

        public async Task<ProductDto?> CreateProductWithImageAsync(
            string name,
            string category,
            decimal price,
            string? description,
            bool isAvailable,
            int stock,
            IFormFile? image)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "name");
            form.Add(new StringContent(category), "category");
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
            string category,
            decimal price,
            string? description,
            bool isAvailable,
            int stock,
            string? existingImageUrl,
            IFormFile? image)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(name), "name");
            form.Add(new StringContent(category), "category");
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
    }
}
