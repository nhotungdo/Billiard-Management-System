using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BilliardManagement.Web.Pages.Admin.Revenue
{
    public class IndexModel : AdminPageModel
    {
        private readonly RevenueService _revenueService;
        private readonly StaffService _staffService;

        public IndexModel(RevenueService revenueService, StaffService staffService)
        {
            _revenueService = revenueService;
            _staffService = staffService;
        }

        public RevenueSummaryDto SummaryData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? StaffId { get; set; }

        public List<SelectListItem> StaffList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
            {
                TempData["ErrorMessage"] = "Từ ngày không được lớn hơn Đến ngày. Hệ thống đã tự động đảo lại khoảng thời gian cho phù hợp.";
                var temp = FromDate;
                FromDate = ToDate;
                ToDate = temp;
            }

            await LoadStaffListAsync();

            try
            {
                SummaryData = await _revenueService.GetTotalRevenueAsync(StaffId, FromDate, ToDate) ?? new();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể tải dữ liệu thống kê: " + ex.Message;
            }

            return Page();
        }

        private async Task LoadStaffListAsync()
        {
            try
            {
                var staffs = await _staffService.GetAllStaffAsync();
                StaffList = staffs?
                    .Where(u => u.Role == 2) // Staff members only
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FullName,
                        Selected = StaffId == u.Id
                    })
                    .ToList() ?? new();
            }
            catch
            {
                StaffList = new();
            }
        }
    }
}
