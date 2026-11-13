using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class StoreService : BaseApiService
    {
        public StoreService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<StoreDto>?> GetAllStoresAsync()
        {
            return await GetAsync<List<StoreDto>>("stores");
        }

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
