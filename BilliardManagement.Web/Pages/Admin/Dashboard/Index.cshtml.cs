using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BilliardManagement.Web.Pages.Admin.Dashboard
{
    public class IndexModel : AdminPageModel
    {
        private readonly RevenueService _revenueService;
        private readonly TableService _tableService;
        private readonly ProductService _productService;
        private readonly StaffService _staffService;
        private readonly SessionService _sessionService;
        private readonly InvoiceService _invoiceService;
        private readonly StoreService _stores;
        public SystemOverviewDto SystemOverview { get; set; } = new();

        public IndexModel(
            RevenueService revenueService,
            TableService tableService,
            ProductService productService,
            StaffService staffService,
            SessionService sessionService,
            InvoiceService invoiceService, StoreService stores)
        {
            _revenueService = revenueService;
            _tableService = tableService;
            _productService = productService;
            _staffService = staffService;
            _sessionService = sessionService;
            _invoiceService = invoiceService;
            _stores = stores;
        }

        // Stats Cards properties
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int ActiveTablesCount { get; set; }
        public int TotalSessionsToday { get; set; }
        public int TotalInvoicesToday { get; set; }
        public int OnlineStaffCount { get; set; }

        // Collections for sections
        public List<TableDashboardDto> LiveTables { get; set; } = new();
        public List<InvoiceDto> RecentInvoices { get; set; } = new();
        public List<StaffRevenueDto> TopStaff { get; set; } = new();

        // Chart Data properties
        public List<RevenueDto> DailyRevenueTrend { get; set; } = new();
        public Dictionary<string, decimal> RevenueByTableType { get; set; } = new();
        public decimal TotalPlayingFee { get; set; }
        public decimal TotalServiceFee { get; set; }

        // Error and Info properties
        public string? ErrorMessage { get; set; }

        // Helper counts from current tables
        public int CountAvailable => LiveTables.Count(t => t.Status == 1);
        public int CountPlaying => LiveTables.Count(t => t.Status == 2);
        public int CountWaiting => LiveTables.Count(t => t.Status == 3);
        public int CountMaintenance => LiveTables.Count(t => t.Status == 4);

        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentRole == "SuperAdmin")
            {
                try
                {
                    SystemOverview = await _stores.GetOverviewAsync() ?? new();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Không thể tải tổng quan hệ thống: {ex.Message}";
                }
                return Page();
            }

            var errors = new List<string>();

            // 1. Fetch dashboard/revenue summary from RevenueService
            try
            {
                var summary = await _revenueService.GetTotalRevenueAsync(null, null, null);
                if (summary != null)
                {
                    TodayRevenue = summary.TodayRevenue;
                    MonthRevenue = summary.MonthRevenue;
                    TotalInvoicesToday = summary.TodayInvoices;
                    TopStaff = summary.StaffRevenues.OrderByDescending(s => s.TotalRevenue).Take(5).ToList();
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Doanh thu: {ex.Message}");
            }

            // 2. Fetch active sessions & live table status
            try
            {
                var liveTables = await _sessionService.GetDashboardAsync();
                LiveTables = liveTables ?? new();
                ActiveTablesCount = LiveTables.Count(t => t.Status == 2); // 2 is Playing
            }
            catch (Exception ex)
            {
                errors.Add($"Trạng thái bàn: {ex.Message}");
            }

            // 3. Fetch today's sessions count (filter by today's date in local/UTC time)
            try
            {
                var todayUtc = DateTime.UtcNow.Date;
                var sessionsResult = await _sessionService.GetPagedSessionsAsync(1, 1000, null, null, null, "StartTime", true);
                TotalSessionsToday = sessionsResult?.Items?.Count(s => s.StartTime.Date == todayUtc) ?? 0;
            }
            catch (Exception ex)
            {
                errors.Add($"Lượt chơi: {ex.Message}");
            }

            // 4. Fetch online staff count (checked-in shifts without CheckOut time)
            try
            {
                var todayShifts = await _staffService.GetTodayShiftsAsync();
                OnlineStaffCount = todayShifts?.Count(s => s.CheckOut == null) ?? 0;
            }
            catch (Exception ex)
            {
                errors.Add($"Nhân viên trực: {ex.Message}");
            }

            // 5. Fetch 10 most recent paid invoices
            try
            {
                var invoicesResult = await _invoiceService.GetPagedInvoicesAsync(1, 10, true, null, "CreatedAt", true);
                RecentInvoices = invoicesResult?.Items ?? new();
            }
            catch (Exception ex)
            {
                errors.Add($"Hóa đơn gần đây: {ex.Message}");
            }

            // 6. Fetch monthly revenue trend (last 30 days)
            try
            {
                var dailyRevenue = await _revenueService.GetMonthlyRevenueAsync();
                DailyRevenueTrend = dailyRevenue ?? new();
            }
            catch (Exception ex)
            {
                errors.Add($"Biểu đồ doanh thu: {ex.Message}");
            }

            // 7. Calculate Revenue by Table Type & Playing vs Services Breakdown from all invoices
            try
            {
                var allInvoicesResult = await _invoiceService.GetAllInvoicesAsync();
                var allInvoices = allInvoicesResult ?? new();

                RevenueByTableType = allInvoices
                    .GroupBy(i => i.TableType ?? "Khác")
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Total));

                TotalPlayingFee = allInvoices.Sum(i => i.PlayingFee);
                TotalServiceFee = allInvoices.Sum(i => i.ServiceFee);
            }
            catch (Exception ex)
            {
                errors.Add($"Cơ cấu doanh thu: {ex.Message}");
            }

            if (errors.Count > 0)
            {
                ErrorMessage = $"Một số dữ liệu chưa tải được: {string.Join("; ", errors)}";
            }

            return Page();
        }
    }
}
