using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/Auth/Login");
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("/Auth/Login");
        }
    }
}
