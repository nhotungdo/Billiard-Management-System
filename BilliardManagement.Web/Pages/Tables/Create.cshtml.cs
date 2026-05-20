using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Tables
{
    public class CreateModel : PageModel
    {
        private readonly TableService _tableService;

        public CreateModel(TableService tableService)
        {
            _tableService = tableService;
        }

        [BindProperty]
        public CreateTableDto TableData { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _tableService.CreateTableAsync(TableData);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Table created successfully.";
                    return RedirectToPage("/Tables/Index");
                }
                else
                {
                    ErrorMessage = "Failed to create table.";
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
