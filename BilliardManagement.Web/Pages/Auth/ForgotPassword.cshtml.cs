using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BilliardManagement.Web.Services;

namespace BilliardManagement.Web.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
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
                var role = HttpContext.Session.GetString("UserRole");
                var home = role switch
                {
                    "SuperAdmin" => "/Admin/Dashboard/Index",
                    "Admin" => "/Admin/Dashboard/Index",
                    "Staff" => "/Staff/Dashboard/Index",
                    _ => "/Auth/Login"
                };
                Response.Redirect(home);
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                PasswordResetStore.AddRequest(UsernameOrPhone, FullName, Note);

                SuccessMessage = "Yêu cầu đã được ghi nhận. Vui lòng liên hệ Admin cửa hàng với tên đăng nhập chính xác để được hỗ trợ; nếu là Admin, hãy liên hệ SuperAdmin.";
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
