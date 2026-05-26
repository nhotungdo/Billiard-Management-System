using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Services
{
    public class CategoryService : BaseApiService
    {
        public CategoryService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<CategoryDto>?> GetAllCategoriesAsync()
        {
            return await GetAsync<List<CategoryDto>>("categories");
        }
    }
}
