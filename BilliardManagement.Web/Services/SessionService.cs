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
            return await GetAsync<List<SessionDto>>("table-sessions/active");
        }

        public async Task<List<TableDashboardDto>?> GetDashboardAsync()
        {
            return await GetAsync<List<TableDashboardDto>>("table-sessions/dashboard");
        }

        public async Task<SessionDto?> StartSessionAsync(Guid tableId, int durationHours, string? customerName = null, string? customerPhone = null, int paymentMethod = 0, Guid? comboId = null)
        {
            var url = "table-sessions/start";
            return await PostAsync<StartSessionRequest, SessionDto>(url,
                new StartSessionRequest 
                { 
                    TableId = tableId, 
                    DurationHours = durationHours, 
                    CustomerName = customerName, 
                    CustomerPhone = customerPhone,
                    PaymentMethod = paymentMethod,
                    ComboId = comboId
                });
        }

        public async Task<SessionDto?> ExtendSessionAsync(Guid sessionId, int additionalMinutes)
        {
            return await PostAsync<ExtendSessionRequest, SessionDto>($"table-sessions/extend/{sessionId}",
                new ExtendSessionRequest { AdditionalMinutes = additionalMinutes });
        }

        public async Task<SessionDto?> EndSessionAsync(Guid sessionId, int paymentMethod = 0)
        {
            return await PostAsync<EndSessionRequest, SessionDto>($"table-sessions/end/{sessionId}",
                new EndSessionRequest { PaymentMethod = paymentMethod });
        }

        public async Task<PagedResult<SessionDto>?> GetPagedSessionsAsync(int pageNumber, int pageSize, int? status = null, Guid? tableId = null, bool? isFinished = null, string? sortBy = null, bool isDescending = false)
        {
            var url = $"table-sessions?pageNumber={pageNumber}&pageSize={pageSize}";
            if (status.HasValue) url += $"&status={status.Value}";
            if (tableId.HasValue) url += $"&tableId={tableId.Value}";
            if (isFinished.HasValue) url += $"&isFinished={isFinished.Value}";
            if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}&isDescending={isDescending}";
            return await GetAsync<PagedResult<SessionDto>>(url);
        }
    }
}
