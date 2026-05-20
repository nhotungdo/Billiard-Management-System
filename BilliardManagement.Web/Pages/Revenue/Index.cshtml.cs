using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Revenue
{
    public class IndexModel : PageModel
    {
        private readonly RevenueService _revenueService;

        public IndexModel(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public List<RevenueDto> DailyRevenue { get; set; } = new();
        public List<RevenueDto> MonthlyRevenue { get; set; } = new();
        public List<RevenueDto> YearlyRevenue { get; set; } = new();
        
        public decimal TotalDaily { get; set; }
        public decimal TotalMonthly { get; set; }
        public decimal TotalYearly { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var daily = await _revenueService.GetDailyRevenueAsync(7);
                if (daily != null)
                {
                    DailyRevenue = daily.OrderBy(r => r.Date).ToList();
                    TotalDaily = DailyRevenue.Sum(r => r.TotalRevenue);
                }

                var monthly = await _revenueService.GetMonthlyRevenueAsync();
                if (monthly != null)
                {
                    MonthlyRevenue = monthly.OrderBy(r => r.Date).ToList();
                    TotalMonthly = MonthlyRevenue.Sum(r => r.TotalRevenue);
                }

                var yearly = await _revenueService.GetYearlyRevenueAsync();
                if (yearly != null)
                {
                    YearlyRevenue = yearly.OrderBy(r => r.Date).ToList();
                    TotalYearly = YearlyRevenue.Sum(r => r.TotalRevenue);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load revenue reports: {ex.Message}";
            }
        }
    }
}
