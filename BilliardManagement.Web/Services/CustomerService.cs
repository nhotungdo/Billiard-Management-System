using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class CustomerService : BaseApiService
    {
        public CustomerService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<PagedResult<CustomerDto>?> GetPagedCustomersAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"customers?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<CustomerDto>>(url);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            return await GetAsync<CustomerDto>($"customers/{id}");
        }

        public async Task<CustomerDto?> CreateCustomerAsync(CustomerCreateDto dto)
        {
            return await PostAsync<CustomerCreateDto, CustomerDto>("customers", dto);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto)
        {
            return await PutAsync<CustomerUpdateDto, CustomerDto>($"customers/{id}", dto);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            return await DeleteAsync($"customers/{id}");
        }

        public async Task<List<CustomerTopSpenderDto>?> GetTopSpendingCustomersAsync(int top = 10)
        {
            return await GetAsync<List<CustomerTopSpenderDto>>($"customers/top-spending?top={top}");
        }

        public async Task<CustomerDashboardDto?> GetCustomerDashboardAsync()
        {
            return await GetAsync<CustomerDashboardDto>("customers/dashboard");
        }
    }
}
