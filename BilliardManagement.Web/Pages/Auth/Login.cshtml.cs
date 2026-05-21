using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginRequest LoginData { get; set; } = new()
        {
            Username = "admin",
            Password = "123456"
        };

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // If already logged in, redirect based on role
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(token))
            {
                if (role == "Admin")
                    Response.Redirect("/Admin/Dashboard/Index");
                else
                    Response.Redirect("/Staff/Dashboard/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var response = await _authService.LoginAsync(LoginData);
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    HttpContext.Session.SetString("JWToken", response.Token);
                    HttpContext.Session.SetString("Username", response.User.Username);
                    var roleName = response.User.Role == 1 ? "Admin" : "Staff";
                    HttpContext.Session.SetString("UserRole", roleName);
                    HttpContext.Session.SetString("FullName", response.User.FullName);
                    HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
                    TempData["SuccessMessage"] = "Đăng nhập thành công!";

                    // Redirect based on role
                    if (roleName == "Admin")
                        return RedirectToPage("/Admin/Dashboard/Index");
                    else
                        return RedirectToPage("/Staff/Dashboard/Index");
                }
                else
                {
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
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
}
