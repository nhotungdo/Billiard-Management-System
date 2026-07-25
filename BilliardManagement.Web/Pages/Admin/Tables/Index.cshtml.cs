using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Tables
{
    public class IndexModel : AdminPageModel
    {
        private readonly TableService _tableService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(TableService tableService, ILogger<IndexModel> logger)
        {
            _tableService = tableService;
            _logger = logger;
        }

        public List<TableDto> Tables { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int CountAvailable => Tables.Count(t => t.Status == 1);
        public int CountPlaying => Tables.Count(t => t.Status == 2);
        public int CountWaiting => Tables.Count(t => t.Status == 3);
        public int CountMaintenance => Tables.Count(t => t.Status == 4);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _tableService.GetAllTablesAsync();
                Tables = result?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách bàn: {Message}", ex.Message);
                ErrorMessage = ex.Message;
            }
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
