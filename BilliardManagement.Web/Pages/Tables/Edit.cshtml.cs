using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Tables
{
    public class EditModel : PageModel
    {
        private readonly TableService _tableService;

        public EditModel(TableService tableService)
        {
            _tableService = tableService;
        }

        [BindProperty]
        public CreateTableDto TableData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                return RedirectToPage("/Tables/Index");
            }

            try
            {
                var table = await _tableService.GetTableByIdAsync(Id);
                if (table == null)
                {
                    TempData["ErrorMessage"] = "Table not found.";
                    return RedirectToPage("/Tables/Index");
                }

                TableData = new CreateTableDto
                {
                    TableName = table.TableName,
                    Type = table.Type,
                    HourlyRate = table.HourlyRate
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load table data: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var success = await _tableService.UpdateTableAsync(Id, TableData);
                if (success)
                {
                    TempData["SuccessMessage"] = "Table updated successfully.";
                    return RedirectToPage("/Tables/Index");
                }
                else
                {
                    ErrorMessage = "Failed to update table.";
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
