using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages
{
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string Message { get; set; } = string.Empty;

        public void OnGet(string message)
        {
            Message = message ?? "Đã xảy ra lỗi không xác định. Vui lòng thử lại sau.";
        }
    }
}
