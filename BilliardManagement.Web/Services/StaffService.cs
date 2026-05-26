using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class StaffService : BaseApiService
    {
        public StaffService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<StaffDto>?> GetAllStaffAsync()
        {
            return await GetAsync<List<StaffDto>>("users");
        }

        public async Task<StaffDto?> GetStaffByIdAsync(Guid id)
        {
            return await GetAsync<StaffDto>($"users/{id}");
        }

        public async Task<StaffDto?> UpdateStaffRoleAsync(Guid id, int role)
        {
            return await PutAsync<int, StaffDto>($"users/{id}/role", role);
        }

        public async Task<UserDeletionResultDto?> DeleteStaffAsync(Guid id)
        {
            return await DeleteAsync<UserDeletionResultDto>($"users/{id}");
        }

        public async Task<List<ShiftDto>?> GetTodayShiftsAsync()
        {
            return await GetAsync<List<ShiftDto>>("shifts/today");
        }
    }
}
