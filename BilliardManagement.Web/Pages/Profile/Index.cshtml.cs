using Microsoft.AspNetCore.Mvc;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using System.Net.Http;
using System.IO;

namespace BilliardManagement.Web.Pages.Profile
{
    public class IndexModel : AdminOrStaffPageModel
    {
        private readonly AuthService _authService;
        private readonly StoreService _stores;
        public StoreDto? Store { get; set; }

        public IndexModel(AuthService authService, StoreService stores)
        {
            _authService = authService;
            _stores = stores;
        }

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public string? PhoneNumber { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public IFormFile? ProfilePicture { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        [BindProperty]
        public ChangePasswordRequest PasswordModel { get; set; } = new();

        [BindProperty]
        public string ActiveTab { get; set; } = "profile";

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var profile = await _authService.GetProfileAsync();
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin cá nhân.";
                    return RedirectToPage("/Dashboard/Index");
                }

                FullName = profile.FullName;
                PhoneNumber = profile.PhoneNumber;
                Email = profile.Email;
                ProfilePictureUrl = profile.ProfilePictureUrl;
                Username = profile.Username;
                Role = profile.Role switch { 0 => "Quản trị hệ thống (SuperAdmin)", 1 => "Quản trị viên (Admin)", _ => "Nhân viên (Staff)" };
                await LoadStoreAsync();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToPage("/Dashboard/Index");
            }
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            ActiveTab = "profile";
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ModelState.AddModelError(nameof(FullName), "Họ tên không được để trống.");
                await RefreshProfileDataAsync();
                return Page();
            }

            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(FullName.Trim()), "FullName");
                content.Add(new StringContent(PhoneNumber?.Trim() ?? ""), "PhoneNumber");
                content.Add(new StringContent(Email?.Trim() ?? ""), "Email");

                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    var fileContent = new StreamContent(ProfilePicture.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ProfilePicture.ContentType);
                    content.Add(fileContent, "ProfilePicture", ProfilePicture.FileName);
                }

                var updatedProfile = await _authService.UpdateProfileAsync(content);
                if (updatedProfile != null)
                {
                    HttpContext.Session.SetString("FullName", updatedProfile.FullName);
                    TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật thông tin cá nhân.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await RefreshProfileDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            ActiveTab = "password";
            if (!ModelState.IsValid)
            {
                await RefreshProfileDataAsync();
                return Page();
            }

            if (PasswordModel.NewPassword != PasswordModel.ConfirmNewPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                await RefreshProfileDataAsync();
                return Page();
            }

            try
            {
                var success = await _authService.ChangePasswordAsync(PasswordModel);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đổi mật khẩu thất bại.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await RefreshProfileDataAsync();
            return Page();
        }

        private async Task RefreshProfileDataAsync()
        {
            try
            {
                var profile = await _authService.GetProfileAsync();
                if (profile != null)
                {
                    ProfilePictureUrl = profile.ProfilePictureUrl;
                    Username = profile.Username;
                    Role = profile.Role switch { 0 => "Quản trị hệ thống (SuperAdmin)", 1 => "Quản trị viên (Admin)", _ => "Nhân viên (Staff)" };
                    await LoadStoreAsync();
                }
            }
            catch
            {
                // Ignore secondary errors on refresh
            }
        }
        private async Task LoadStoreAsync()
        {
            if (CurrentRole != "SuperAdmin")
            {
                Store = await _stores.GetCurrentAsync();
                if (Store != null)
                {
                    HttpContext.Session.SetString("StoreName", Store.Name);
                    HttpContext.Session.SetString("StoreLogoUrl", Store.LogoUrl ?? "");
                }
            }
        }
    }
}
