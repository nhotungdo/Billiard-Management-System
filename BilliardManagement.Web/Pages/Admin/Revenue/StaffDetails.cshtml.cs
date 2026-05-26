using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Revenue
{
    public class StaffDetailsModel : AdminPageModel
    {
        private readonly RevenueService _revenueService;

        public StaffDetailsModel(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public PersonalRevenueDto StaffRevenueData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid StaffId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (StaffId == Guid.Empty) return RedirectToPage("/Admin/Revenue/Index");

            try
            {
                StaffRevenueData = await _revenueService.GetStaffRevenueAsync(StaffId, FromDate, ToDate) ?? new();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Admin/Revenue/Index");
            }

            return Page();
        }
    }
}
