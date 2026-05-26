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

        public IndexModel(
            RevenueService revenueService,
            TableService tableService,
            ProductService productService,
            StaffService staffService,
            SessionService sessionService,
            InvoiceService invoiceService)
        {
            _revenueService = revenueService;
            _tableService = tableService;
            _productService = productService;
            _staffService = staffService;
            _sessionService = sessionService;
            _invoiceService = invoiceService;
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
        public int CountReserved => LiveTables.Count(t => t.Status == 3);
        public int CountMaintenance => LiveTables.Count(t => t.Status == 4);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // 1. Fetch dashboard/revenue summary from RevenueService
                var summary = await _revenueService.GetTotalRevenueAsync(null, null, null);
                if (summary != null)
                {
                    TodayRevenue = summary.TodayRevenue;
                    MonthRevenue = summary.MonthRevenue;
                    TotalInvoicesToday = summary.TodayInvoices;
                    TopStaff = summary.StaffRevenues.OrderByDescending(s => s.TotalRevenue).Take(5).ToList();
                }

                // 2. Fetch active sessions & live table status
                var liveTables = await _sessionService.GetDashboardAsync();
                LiveTables = liveTables ?? new();
                ActiveTablesCount = LiveTables.Count(t => t.Status == 2); // 2 is Playing

                // 3. Fetch today's sessions count (filter by today's date in local/UTC time)
                var todayUtc = DateTime.UtcNow.Date;
                var sessionsResult = await _sessionService.GetPagedSessionsAsync(1, 1000, null, null, null, "StartTime", true);
                TotalSessionsToday = sessionsResult?.Items?.Count(s => s.StartTime.Date == todayUtc) ?? 0;

                // 4. Fetch online staff count (checked-in shifts without CheckOut time)
                var todayShifts = await _staffService.GetTodayShiftsAsync();
                OnlineStaffCount = todayShifts?.Count(s => s.CheckOut == null) ?? 0;

                // 5. Fetch 10 most recent paid invoices
                var invoicesResult = await _invoiceService.GetPagedInvoicesAsync(1, 10, true, null, "CreatedAt", true);
                RecentInvoices = invoicesResult?.Items ?? new();

                // 6. Fetch monthly revenue trend (last 30 days)
                var dailyRevenue = await _revenueService.GetMonthlyRevenueAsync();
                DailyRevenueTrend = dailyRevenue ?? new();

                // 7. Calculate Revenue by Table Type & Playing vs Services Breakdown from all invoices
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
                ErrorMessage = $"Không thể tải dữ liệu Dashboard: {ex.Message}";
            }

            return Page();
        }
    }
}
