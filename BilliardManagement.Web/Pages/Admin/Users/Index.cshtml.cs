using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Users
{
    public class IndexModel : SuperAdminPageModel
    {
        private readonly StoreService _stores;
        public IndexModel(StoreService stores) => _stores = stores;
        public List<UserDto> Users { get; private set; } = new();
        public List<StoreDto> Stores { get; private set; } = new();
        public string? ErrorMessage { get; private set; }
        public string? ActiveModal { get; private set; }
        public Guid SelectedUserId { get; private set; }
        [BindProperty] public CreateStoreAdminRequest NewAdmin { get; set; } = new();
        [BindProperty] public UpdateStoreAdminRequest EditAdmin { get; set; } = new();
        [BindProperty] public ResetPasswordForUserDto PasswordReset { get; set; } = new();
        [BindProperty(SupportsGet = true)] public Guid? StoreId { get; set; }
        [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }

        public async Task<IActionResult> OnPostEditAsync(Guid userId)
        {
            ModelState.Clear();
            TryValidateModel(EditAdmin, nameof(EditAdmin));
            if (!ModelState.IsValid) { ActiveModal = "editAdminModal"; SelectedUserId = userId; await LoadAsync(); return Page(); }
            try { await _stores.UpdateAdminAsync(userId, EditAdmin); TempData["SuccessMessage"] = "Đã cập nhật Admin."; }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToPage(new { StoreId, IsActive });
        }

        public async Task<IActionResult> OnPostToggleAsync(Guid userId, bool active)
        {
            try { await _stores.SetAdminActiveAsync(userId, active); TempData["SuccessMessage"] = "Đã cập nhật trạng thái tài khoản."; }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToPage(new { StoreId, IsActive });
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(Guid userId)
        {
            ModelState.Clear();
            TryValidateModel(PasswordReset, nameof(PasswordReset));
            if (!ModelState.IsValid) { ActiveModal = "resetAdminModal"; SelectedUserId = userId; await LoadAsync(); return Page(); }
            try
            {
                var user = (await _stores.GetUsersAsync())?.FirstOrDefault(u => u.Id == userId && u.Role == 1);
                if (user == null) throw new Exception("Không tìm thấy Admin cửa hàng.");
                await _stores.ResetAdminPasswordAsync(userId, PasswordReset);
                TempData["SuccessMessage"] = "Đã đặt lại mật khẩu Admin.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToPage(new { StoreId, IsActive });
        }

        public async Task OnGetAsync() => await LoadAsync();

        public async Task<IActionResult> OnPostAssignAsync(Guid userId, Guid storeId)
        {
            if (userId == Guid.Empty || storeId == Guid.Empty)
                TempData["ErrorMessage"] = "Vui lòng chọn tài khoản và cửa hàng.";
            else
            {
                try
                {
                    await _stores.AssignAdminAsync(userId, storeId);
                    TempData["SuccessMessage"] = "Đã gán Admin vào cửa hàng. Tài khoản cần đăng nhập lại để nhận quyền mới.";
                }
                catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            ModelState.Clear();
            TryValidateModel(NewAdmin, nameof(NewAdmin));
            if (NewAdmin.StoreId == Guid.Empty)
                ModelState.AddModelError("NewAdmin.StoreId", "Vui lòng chọn cửa hàng.");
            if (!ModelState.IsValid) { ActiveModal = "createAdminModal"; await LoadAsync(); return Page(); }
            try
            {
                await _stores.CreateStoreAdminAsync(NewAdmin);
                TempData["SuccessMessage"] = "Đã tạo Admin cửa hàng.";
                return RedirectToPage();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; ActiveModal = "createAdminModal"; await LoadAsync(); return Page(); }
        }

        private async Task LoadAsync()
        {
            try
            {
                Users = (await _stores.GetUsersAsync() ?? new()).Where(u => u.Role == 1 &&
                    (!StoreId.HasValue || u.StoreId == StoreId) && (!IsActive.HasValue || u.IsActive == IsActive)).ToList();
                Stores = await _stores.GetAllStoresAsync() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }
    }
}
