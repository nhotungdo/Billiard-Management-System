using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Tables
{
    public class IndexModel : AdminOrStaffPageModel
    {
        private readonly TableService _tableService;

        public IndexModel(TableService tableService) => _tableService = tableService;

        public List<TableDto> Tables { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public int CountAvailable => Tables.Count(t => t.Status == 1);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Tables = (await _tableService.GetAllTablesAsync())?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }

        [BindProperty]
        public CreateTableDto EditTable { get; set; } = new();

        [BindProperty]
        public Guid EditId { get; set; }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var success = await _tableService.UpdateTableAsync(EditId, EditTable);
                if (success)
                    TempData["SuccessMessage"] = "Cập nhật bàn thành công.";
                else
                    TempData["ErrorMessage"] = "Không thể cập nhật bàn.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi cập nhật bàn: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _tableService.DeleteTableAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "Xóa bàn thành công.";
                else
                    TempData["ErrorMessage"] = "Không thể xóa bàn.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xóa bàn: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
