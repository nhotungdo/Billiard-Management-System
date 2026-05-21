using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages.Products
{
    public class CreateModel : PageModel
    {
        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            if (role != "Admin" && role != "Staff")
                return RedirectToPage("/AccessDenied");
            return Page();
        }
    }
}
