using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Revenue
{
    public class IndexModel : AdminPageModel
    {
        private readonly RevenueService _revenueService;
        public IndexModel(RevenueService revenueService) { _revenueService = revenueService; }

        public List<RevenueDto> DailyRevenue { get; set; } = new();
        public List<RevenueDto> MonthlyRevenue { get; set; } = new();
        public decimal TotalRevenue => DailyRevenue.Sum(r => r.TotalRevenue);
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var daily = await _revenueService.GetDailyRevenueAsync(30);
                DailyRevenue = daily?.ToList() ?? new();
                MonthlyRevenue = DailyRevenue;
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
