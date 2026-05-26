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
            var startStr = Uri.EscapeDataString(startDate.ToString("o"));
            var endStr = Uri.EscapeDataString(endDate.ToString("o"));
            return await GetAsync<List<RevenueDto>>($"reports/revenue?startDate={startStr}&endDate={endStr}&groupType={Uri.EscapeDataString(groupType)}");
        }

        public async Task<PersonalRevenueDto?> GetPersonalRevenueAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = BuildRevenueQuery(null, fromDate, toDate);
            return await GetAsync<PersonalRevenueDto>($"revenues/personal{query}");
        }

        public async Task<RevenueSummaryDto?> GetTotalRevenueAsync(Guid? staffId, DateTime? fromDate, DateTime? toDate)
        {
            var query = BuildRevenueQuery(staffId, fromDate, toDate);
            return await GetAsync<RevenueSummaryDto>($"revenues/total{query}");
        }

        public async Task<PersonalRevenueDto?> GetStaffRevenueAsync(Guid staffId, DateTime? fromDate, DateTime? toDate)
        {
            var query = BuildRevenueQuery(null, fromDate, toDate);
            return await GetAsync<PersonalRevenueDto>($"revenues/staff/{staffId}{query}");
        }

        private static string BuildRevenueQuery(Guid? staffId, DateTime? fromDate, DateTime? toDate)
        {
            var queryParams = new List<string>();
            if (staffId.HasValue) queryParams.Add($"staffId={staffId.Value}");
            if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("o"))}");
            if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("o"))}");

            return queryParams.Count == 0 ? string.Empty : $"?{string.Join("&", queryParams)}";
        }
    }
}
