using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Revenue
{
    public class IndexModel : StaffPageModel
    {
        private readonly RevenueService _revenueService;

        public IndexModel(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public PersonalRevenueDto RevenueData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                RevenueData = await _revenueService.GetPersonalRevenueAsync(FromDate, ToDate) ?? new();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể tải báo cáo doanh thu cá nhân: " + ex.Message;
            }

            return Page();
        }
    }
}
