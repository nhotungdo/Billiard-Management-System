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
            return await GetAsync<List<TableDto>>("tables");
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

        public async Task<bool> DeleteTableAsync(Guid id)
        {
            return await DeleteAsync($"tables/{id}");
        }
    }
}
