using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class ComboService : BaseApiService
    {
        public ComboService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<ComboDto>?> GetAllCombosAsync(string? search = null, bool? activeOnly = null)
        {
            var url = "combos";
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            if (activeOnly.HasValue) query.Add($"activeOnly={activeOnly.Value.ToString().ToLower()}");
            if (query.Count > 0) url += "?" + string.Join("&", query);

            return await GetAsync<List<ComboDto>>(url);
        }

        public async Task<ComboDto?> GetComboByIdAsync(Guid id)
        {
            return await GetAsync<ComboDto>($"combos/{id}");
        }

        public async Task<ComboDto?> GetComboByCodeAsync(string code)
        {
            return await GetAsync<ComboDto>($"combos/code/{Uri.EscapeDataString(code)}");
        }

        public async Task<ComboDto?> CreateComboAsync(CreateComboDto request)
        {
            return await PostAsync<CreateComboDto, ComboDto>("combos", request);
        }

        public async Task<bool> UpdateComboAsync(Guid id, CreateComboDto request)
        {
            return await PutAsync($"combos/{id}", request);
        }

        public async Task<bool> ToggleComboStatusAsync(Guid id)
        {
            return await PatchAsync($"combos/{id}/toggle-status");
        }

        public async Task<bool> DeleteComboAsync(Guid id)
        {
            return await DeleteAsync($"combos/{id}");
        }

        public async Task<ApplyComboResultDto?> ApplyComboToSessionAsync(ApplyComboRequestDto request)
        {
            return await PostAsync<ApplyComboRequestDto, ApplyComboResultDto>("combos/apply", request);
        }
    }
}
