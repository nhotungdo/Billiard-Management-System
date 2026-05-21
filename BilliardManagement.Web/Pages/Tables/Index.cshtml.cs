using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Tables
{
    public class IndexModel : PageModel
    {
        private readonly TableService _tableService;

        public IndexModel(TableService tableService)
        {
            _tableService = tableService;
        }

        public List<TableDto> Tables { get; set; } = new();

        [BindProperty]
        public CreateTableDto EditTable { get; set; } = new();

        [BindProperty]
        public Guid EditId { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var list = await _tableService.GetAllTablesAsync();
                if (list != null)
                {
                    Tables = list;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load tables: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var list = await _tableService.GetAllTablesAsync();
                    if (list != null) Tables = list;
                }
                catch { }
                return Page();
            }

            try
            {
                var success = await _tableService.UpdateTableAsync(EditId, EditTable);
                if (success)
                {
                    TempData["SuccessMessage"] = "Table updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update table.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating table: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _tableService.DeleteTableAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Table deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete table.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting table: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
