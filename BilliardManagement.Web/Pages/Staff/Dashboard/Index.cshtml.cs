using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Dashboard
{
    public class IndexModel : StaffPageModel
    {
        private readonly SessionService _sessionService;
        private readonly OrderService _orderService;
        private readonly InvoiceService _invoiceService;
        private readonly ProductService _productService;

        public IndexModel(
            SessionService sessionService,
            OrderService orderService,
            InvoiceService invoiceService,
            ProductService productService)
        {
            _sessionService = sessionService;
            _orderService = orderService;
            _invoiceService = invoiceService;
            _productService = productService;
        }

        public List<TableDashboardDto> TableDashboard { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public List<OrderDto> TodayOrders { get; set; } = new();
        public List<InvoiceDto> TodayInvoices { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int CountAvailable => TableDashboard.Count(t => t.Status == 1);
        public int CountPlaying => TableDashboard.Count(t => t.Status == 2);
        public decimal TodayRevenue => TodayInvoices.Where(i => i.IsPaid).Sum(i => i.Total);

        public static DateTime ToUtc(DateTime dt) =>
            dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };

        public static DateTime GetEndUtc(SessionDto s)
        {
            if (s.EndTime.HasValue)
                return ToUtc(s.EndTime.Value);
            var hours = s.DurationHours > 0 ? s.DurationHours : 1;
            return ToUtc(s.StartTime).AddHours(hours);
        }

        public static int GetRemainingSeconds(SessionDto? s)
        {
            if (s == null) return 0;
            var end = GetEndUtc(s);
            return Math.Max(0, (int)(end - DateTime.UtcNow).TotalSeconds);
        }

        public static string FormatTimer(int totalSeconds)
        {
            var h = totalSeconds / 3600;
            var m = (totalSeconds % 3600) / 60;
            var sec = totalSeconds % 60;
            return $"{h:D2}:{m:D2}:{sec:D2}";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var dashboard = await _sessionService.GetDashboardAsync();
                TableDashboard = dashboard?.ToList() ?? new();

                var products = await _productService.GetAllProductsAsync();
                Products = products?.Where(p => p.Stock > 0).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList() ?? new();

                var orders = await _orderService.GetAllOrdersAsync();
                TodayOrders = orders?.ToList() ?? new();

                var invoices = await _invoiceService.GetAllInvoicesAsync();
                TodayInvoices = invoices?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
            }

            return Page();
        }
    }
}
