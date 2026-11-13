using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Staff
{
    public class IndexModel : AdminPageModel
    {
        private readonly StaffService _staffService;
        public IndexModel(StaffService staffService)
        {
            _staffService = staffService;
        }

        public List<StaffDto> StaffList { get; set; } = new();
        public List<PasswordResetItem> PendingPasswordResetRequests { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateUserRequest NewStaff { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _staffService.GetAllStaffAsync();
                StaffList = result?.ToList() ?? new();
                PendingPasswordResetRequests = PasswordResetStore.GetPendingRequests(
                    StaffList.Where(s => s.Role == 2).Select(s => s.Username));
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
                await _staffService.CreateStaffAsync(new CreateUserRequest
                {
                    FullName = NewStaff.FullName,
                    Username = NewStaff.Username,
                    Password = NewStaff.Password,
                    PhoneNumber = NewStaff.PhoneNumber,
                    Email = NewStaff.Email,
                    Role = 2,
                    StoreId = CurrentRole == "SuperAdmin" && Guid.TryParse(HttpContext.Session.GetString("ViewAsStoreId"), out var storeId) ? storeId : null
                });
                TempData["SuccessMessage"] = "Tạo nhân viên mới thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo nhân viên: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivateAsync(Guid id)
        {
            try
            {
                await _staffService.SetActiveAsync(id, true);
                TempData["SuccessMessage"] = "Đã kích hoạt lại tài khoản nhân viên.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi kích hoạt nhân viên: {ex.Message}";
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

        public async Task<IActionResult> OnPostResolvePasswordResetAsync(Guid requestId)
        {
            try
            {
                var staff = await _staffService.GetAllStaffAsync() ?? new();
                if (!PasswordResetStore.MarkAsResolved(requestId, staff.Where(s => s.Role == 2).Select(s => s.Username)))
                    return NotFound();
                TempData["SuccessMessage"] = "Đã đánh dấu xử lý xong yêu cầu cấp lại mật khẩu.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToPage();
        }
    }
}
