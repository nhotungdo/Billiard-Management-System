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

            try
            {
                var response = await _authService.LoginAsync(LoginData);
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    HttpContext.Session.SetString("JWToken", response.Token);
                    HttpContext.Session.SetString("Username", response.User.Username);
                    HttpContext.Session.SetString("UserRole", response.User.Role.ToString());
                    HttpContext.Session.SetString("FullName", response.User.FullName);
                    TempData["SuccessMessage"] = "Logged in successfully!";
                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
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
