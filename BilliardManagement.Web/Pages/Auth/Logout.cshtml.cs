using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
