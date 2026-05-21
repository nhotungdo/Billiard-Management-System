using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly RevenueService _revenueService;

        public IndexModel(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public DashboardDto? DashboardData { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                DashboardData = await _revenueService.GetDashboardAnalyticsAsync();
                if (DashboardData == null)
                {
                    DashboardData = new DashboardDto
                    {
                        TotalRevenue = 0,
                        ActiveTables = 0,
                        OrdersToday = 0,
                        TopCustomer = "N/A"
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load dashboard data: {ex.Message}";
            }

            return Page();
        }
    }
}
