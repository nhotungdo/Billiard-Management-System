using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Staff
{
    public class IndexModel : AdminPageModel
    {
        private readonly StaffService _staffService;
        private readonly AuthService _authService;
        public IndexModel(StaffService staffService, AuthService authService)
        {
            _staffService = staffService;
            _authService = authService;
        }

        public List<StaffDto> StaffList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public RegisterRequest NewStaff { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _staffService.GetAllStaffAsync();
                StaffList = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToPage();
            }

            try
            {
                await _authService.RegisterAsync(NewStaff);
                TempData["SuccessMessage"] = "Tạo nhân viên mới thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo nhân viên: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);
                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Đã xóa nhân viên thành công. Số khách hàng đã phục vụ: {result.CustomersServed}, Số mặt hàng đã bán: {result.ItemsSold}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa nhân viên: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
