using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BilliardManagement.Web.Services;

namespace BilliardManagement.Web.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập hoặc Số điện thoại")]
        public string UsernameOrPhone { get; set; } = string.Empty;

        [BindProperty]
        public string? FullName { get; set; }

        [BindProperty]
        public string? Note { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
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

            try
            {
                PasswordResetStore.AddRequest(UsernameOrPhone, FullName, Note);

                SuccessMessage = $"Yêu cầu cấp lại mật khẩu của bạn (Tài khoản/SĐT: {UsernameOrPhone}) đã được gửi thành công đến Quản trị viên (Admin). Vui lòng liên hệ Admin để nhận mật khẩu mới.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
