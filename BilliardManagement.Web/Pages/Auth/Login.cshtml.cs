using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly StoreService _stores;

        public LoginModel(AuthService authService, StoreService stores)
        {
            _authService = authService;
            _stores = stores;
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
                if (role == "SuperAdmin")
                    Response.Redirect("/Admin/Dashboard/Index");
                else if (role == "Admin")
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
                    HttpContext.Session.Clear();
                    HttpContext.Session.SetString("JWToken", response.Token);
                    HttpContext.Session.SetString("Username", response.User.Username);
                    var roleName = response.User.Role switch
                    {
                        0 => "SuperAdmin",
                        1 => "Admin",
                        _ => "Staff"
                    };
                    HttpContext.Session.SetString("UserRole", roleName);
                    HttpContext.Session.SetString("FullName", response.User.FullName);
                    HttpContext.Session.SetString("UserId", response.User.Id.ToString());
                    HttpContext.Session.SetString("StoreId", response.User.StoreId?.ToString() ?? "");
                    HttpContext.Session.SetString("StoreName", response.User.StoreName ?? "");
                    if (roleName != "SuperAdmin")
                    {
                        try
                        {
                            var store = await _stores.GetCurrentAsync();
                            if (store != null)
                            {
                                HttpContext.Session.SetString("StoreName", store.Name);
                                HttpContext.Session.SetString("StoreLogoUrl", store.LogoUrl ?? "");
                            }
                        }
                        catch { /* Store branding is optional; authentication already succeeded. */ }
                    }
                    HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
                    TempData["SuccessMessage"] = "Đăng nhập thành công!";

                    // Redirect based on role
                    if (roleName == "SuperAdmin")
                        return RedirectToPage("/Admin/Dashboard/Index");
                    if (roleName == "Admin")
                        return RedirectToPage("/Admin/Dashboard/Index");
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
