using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Services
{
    public class RevenueService : BaseApiService
    {
        public RevenueService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory, httpContextAccessor)
        {
        }

        public async Task<DashboardDto?> GetDashboardAnalyticsAsync()
        {
            return await GetAsync<DashboardDto>("reports/dashboard");
        }

        public async Task<List<RevenueDto>?> GetDailyRevenueAsync(int days = 7)
        {
            return await GetAsync<List<RevenueDto>>($"reports/revenue/daily?days={days}");
        }

        public async Task<List<RevenueDto>?> GetMonthlyRevenueAsync()
        {
            return await GetAsync<List<RevenueDto>>("reports/revenue/daily?days=30");
        }

        public async Task<List<RevenueDto>?> GetYearlyRevenueAsync()
        {
            return await GetAsync<List<RevenueDto>>("reports/revenue/daily?days=365");
        }

        public async Task<List<RevenueDto>?> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupType)
        {
            var startStr = startDate.ToString("o");
            var endStr = endDate.ToString("o");
            return await GetAsync<List<RevenueDto>>($"reports/revenue?startDate={startStr}&endDate={endStr}&groupType={groupType}");
        }
    }
}
