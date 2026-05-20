using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class SessionService : BaseApiService
    {
        public SessionService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<List<SessionDto>?> GetActiveSessionsAsync()
        {
            return await GetAsync<List<SessionDto>>("sessions/active");
        }

        public async Task<SessionDto?> StartSessionAsync(Guid tableId)
        {
            return await PostWithoutBodyAsync<SessionDto>($"sessions/start/{tableId}");
        }

        public async Task<SessionDto?> EndSessionAsync(Guid sessionId)
        {
            return await PostWithoutBodyAsync<SessionDto>($"sessions/end/{sessionId}");
        }
    }
}
