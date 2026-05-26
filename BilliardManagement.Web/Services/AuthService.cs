using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace BilliardManagement.Web.Services
{
    public class AuthService : BaseApiService
    {
        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            return await PostAsync<LoginRequest, LoginResponse>("auth/login", request);
        }

        public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
        {
            return await PostAsync<RegisterRequest, LoginResponse>("auth/register", request);
        }

        public async Task<StaffDto?> GetProfileAsync()
        {
            return await GetAsync<StaffDto>("users/profile");
        }

        public async Task<StaffDto?> UpdateProfileAsync(MultipartFormDataContent content)
        {
            return await PutMultipartAsync<StaffDto>("users/profile", content);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            return await PutAsync<ChangePasswordRequest>("users/change-password", request);
        }
    }
}
