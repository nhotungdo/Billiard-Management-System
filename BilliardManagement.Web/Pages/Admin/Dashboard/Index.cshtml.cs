using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Dashboard
{
    public class IndexModel : AdminPageModel
    {
        private readonly RevenueService _revenueService;
        private readonly TableService _tableService;
        private readonly ProductService _productService;
        private readonly StaffService _staffService;

        public IndexModel(RevenueService revenueService, TableService tableService, ProductService productService, StaffService staffService)
        {
            _revenueService = revenueService;
            _tableService = tableService;
            _productService = productService;
            _staffService = staffService;
        }

        public DashboardDto? DashboardData { get; set; }
        public List<TableDto> Tables { get; set; } = new();
        public List<StaffDto> StaffList { get; set; } = new();
        public List<RevenueDto> RevenueChart { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int TotalTables => Tables.Count;
        public int CountAvailable => Tables.Count(t => t.Status == 1);
        public int CountPlaying => Tables.Count(t => t.Status == 2);
        public int CountReserved => Tables.Count(t => t.Status == 3);
        public int CountMaintenance => Tables.Count(t => t.Status == 4);
        public int ActiveTables => CountPlaying;
        public int StaffCount => StaffList.Count(s => s.IsActive);
        public int TotalProducts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                DashboardData = await _revenueService.GetDashboardAnalyticsAsync();
                DashboardData ??= new DashboardDto { TotalRevenue = 0, ActiveTables = 0, OrdersToday = 0, TopCustomer = "N/A" };

                var tables = await _tableService.GetAllTablesAsync();
                Tables = tables?.ToList() ?? new();

                var products = await _productService.GetAllProductsAsync();
                TotalProducts = products?.Count ?? 0;

                var staff = await _staffService.GetAllStaffAsync();
                StaffList = staff?.ToList() ?? new();

                var revenue = await _revenueService.GetMonthlyRevenueAsync();
                RevenueChart = revenue?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
            }

            return Page();
        }
    }
}
