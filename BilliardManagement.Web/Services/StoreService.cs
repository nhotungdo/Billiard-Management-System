using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class StoreService : BaseApiService
    {
        protected override void AttachToken()
        {
            base.AttachToken();
            _httpClient.DefaultRequestHeaders.Remove("X-Store-Id");
        }
        public StoreService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<StoreDto>?> GetAllStoresAsync()
        {
            return await GetAsync<List<StoreDto>>("stores");
        }

        public Task<List<UserDto>?> GetUsersAsync() => GetAsync<List<UserDto>>("users");

        public Task<StoreDto?> GetCurrentAsync() => GetAsync<StoreDto>("stores/current");
        public Task<SystemOverviewDto?> GetOverviewAsync() => GetAsync<SystemOverviewDto>("reports/system-overview");
        public Task<List<StoreReportDto>?> CompareAsync(IEnumerable<Guid>? ids, DateTime from, DateTime to)
        {
            var query = $"reports/compare?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            if (ids != null) foreach (var id in ids) query += $"&storeIds={id}";
            return GetAsync<List<StoreReportDto>>(query);
        }
        public Task<UserDto?> UpdateAdminAsync(Guid id, UpdateStoreAdminRequest request) =>
            PutAsync<UpdateStoreAdminRequest, UserDto>($"users/{id}/admin", request);
        public Task<UserDto?> SetAdminActiveAsync(Guid id, bool active) =>
            PutAsync<bool, UserDto>($"users/{id}/active", active);
        public Task<string?> ResetAdminPasswordAsync(Guid id, ResetPasswordForUserDto request) =>
            PostAsync<ResetPasswordForUserDto, string>($"users/{id}/reset-password", request);

        public Task<UserDto?> AssignAdminAsync(Guid userId, Guid storeId) =>
            PutAsync<Guid, UserDto>($"users/{userId}/store", storeId);

        public async Task<StoreDto?> GetStoreByIdAsync(Guid id)
        {
            return await GetAsync<StoreDto>($"stores/{id}");
        }

        public async Task<StoreDto?> CreateStoreAsync(CreateStoreRequest request)
        {
            return await PostAsync<CreateStoreRequest, StoreDto>("stores", request);
        }

        public async Task<StoreDto?> UpdateStoreAsync(Guid id, UpdateStoreRequest request)
        {
            return await PutAsync<UpdateStoreRequest, StoreDto>($"stores/{id}", request);
        }

        public async Task<bool> DeactivateStoreAsync(Guid id)
        {
            return await DeleteAsync($"stores/{id}");
        }

        public async Task<StaffDto?> CreateStoreAdminAsync(CreateStoreAdminRequest request)
        {
            var payload = new CreateUserRequest
            {
                FullName = request.FullName,
                Username = request.Username,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Role = 1,
                StoreId = request.StoreId
            };
            return await PostAsync<CreateUserRequest, StaffDto>("users", payload);
        }
    }
}
