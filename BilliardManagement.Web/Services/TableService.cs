using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class TableService : BaseApiService
    {
        public TableService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<TableDto>?> GetAllTablesAsync()
        {
            var result = await GetAsync<PagedResult<TableDto>>("tables?pageSize=1000");
            return result?.Items;
        }

        public async Task<PagedResult<TableDto>?> GetPagedTablesAsync(int pageNumber, int pageSize, int? status = null, string? tableType = null, string? searchTerm = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"tables?pageNumber={pageNumber}&pageSize={pageSize}";
            if (status.HasValue) url += $"&status={status.Value}";
            if (!string.IsNullOrEmpty(tableType)) url += $"&tableType={Uri.EscapeDataString(tableType)}";
            if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<TableDto>>(url);
        }

        public async Task<TableDto?> GetTableByIdAsync(Guid id)
        {
            return await GetAsync<TableDto>($"tables/{id}");
        }

        public async Task<TableDto?> CreateTableAsync(CreateTableDto request)
        {
            return await PostAsync<CreateTableDto, TableDto>("tables", request);
        }

        public async Task<bool> UpdateTableAsync(Guid id, CreateTableDto request)
        {
            return await PutAsync($"tables/{id}", request);
        }

        public async Task<TableDto?> UpdateTableStatusAsync(Guid id, string status)
        {
            return await PutAsync<UpdateTableStatusRequest, TableDto>($"tables/{id}/status", new UpdateTableStatusRequest { Status = status });
        }

        public async Task<bool> DeleteTableAsync(Guid id)
        {
            return await DeleteAsync($"tables/{id}");
        }
    }
}
