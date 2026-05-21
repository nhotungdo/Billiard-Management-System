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
        public int CountReserved => Tables.Count(t => t.Status == 3);
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
    }
}
