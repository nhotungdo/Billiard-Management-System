using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace BilliardManagement.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly AuthService _authService;

        public RegisterModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel RegisterData { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // If already logged in, redirect to Dashboard
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (RegisterData.Password != RegisterData.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return Page();
            }

            try
            {
                var request = new RegisterRequest
                {
                    FullName = RegisterData.FullName,
                    Username = RegisterData.Username,
                    Password = RegisterData.Password,
                    PhoneNumber = RegisterData.PhoneNumber
                };

                var response = await _authService.RegisterAsync(request);
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Log in immediately upon successful registration
                    HttpContext.Session.SetString("JWToken", response.Token);
                    HttpContext.Session.SetString("Username", response.User.Username);
                    HttpContext.Session.SetString("UserRole", response.User.Role == 1 ? "Admin" : "Staff");
                    HttpContext.Session.SetString("FullName", response.User.FullName);
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    ErrorMessage = "Không thể đăng ký tài khoản. Vui lòng thử lại.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên cơ sở / Câu lạc bộ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên quá dài")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
