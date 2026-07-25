using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Pages.Staff.Dashboard
{
    public class IndexModel : StaffPageModel
    {
        private readonly SessionService _sessionService;
        private readonly OrderService _orderService;
        private readonly RevenueService _revenueService;
        private readonly ProductService _productService;
        private readonly ComboService _comboService;

        public IndexModel(
            SessionService sessionService,
            OrderService orderService,
            RevenueService revenueService,
            ProductService productService,
            ComboService comboService)
        {
            _sessionService = sessionService;
            _orderService = orderService;
            _revenueService = revenueService;
            _productService = productService;
            _comboService = comboService;
        }

        // Stats
        public decimal MyRevenueToday { get; set; }
        public int SessionsHandledToday { get; set; }
        public int ActiveTablesCount { get; set; }
        public int OrdersTodayCount { get; set; }

        // Collections
        public List<TableDashboardDto> TableDashboard { get; set; } = new();
        public List<TableDashboardDto> MyActiveSessions { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public List<ComboDto> Combos { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();
        public decimal[] HourlyRevenueData { get; set; } = new decimal[24];
        public string? ErrorMessage { get; set; }

        public class ActivityItem
        {
            public string Action { get; set; } = string.Empty;
            public DateTime Time { get; set; }
            public string Icon { get; set; } = string.Empty;
            public string BadgeClass { get; set; } = string.Empty;
        }

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

        private Guid? GetCurrentStaffId()
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && Guid.TryParse(sessionUserId, out var guid))
            {
                return guid;
            }

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var parts = token.Split('.');
                if (parts.Length >= 2)
                {
                    var payloadBase64 = parts[1];
                    payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');
                    switch (payloadBase64.Length % 4)
                    {
                        case 2: payloadBase64 += "=="; break;
                        case 3: payloadBase64 += "="; break;
                    }
                    var payloadBytes = Convert.FromBase64String(payloadBase64);
                    var json = System.Text.Encoding.UTF8.GetString(payloadBytes);
                    using (var doc = System.Text.Json.JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("nameid", out var p1))
                        {
                            if (Guid.TryParse(p1.GetString(), out var g1))
                            {
                                HttpContext.Session.SetString("UserId", g1.ToString());
                                return g1;
                            }
                        }
                        if (doc.RootElement.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var p2))
                        {
                            if (Guid.TryParse(p2.GetString(), out var g2))
                            {
                                HttpContext.Session.SetString("UserId", g2.ToString());
                                return g2;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var staffId = GetCurrentStaffId();
                if (!staffId.HasValue)
                {
                    return RedirectToPage("/Auth/Login");
                }

                // 1. Fetch tables & active sessions
                var dashboard = await _sessionService.GetDashboardAsync();
                TableDashboard = dashboard?.OrderBy(d => d.TableName).ToList() ?? new();

                MyActiveSessions = TableDashboard
                    .Where(t => t.ActiveSession != null && t.ActiveSession.UserId == staffId.Value)
                    .ToList();

                ActiveTablesCount = MyActiveSessions.Count;

                // 2. Fetch products for Quick Order popup & combos
                var products = await _productService.GetAllProductsAsync();
                Products = products?.Where(p => p.Stock > 0 && p.IsAvailable).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList() ?? new();

                var combos = await _comboService.GetAllCombosAsync(activeOnly: true);
                Combos = combos ?? new();

                // 3. Fetch personal revenue analytics
                var personalRev = await _revenueService.GetPersonalRevenueAsync(null, null);
                if (personalRev != null)
                {
                    MyRevenueToday = personalRev.TodayRevenue;

                    // Compute hourly trend for today
                    var todayLocal = DateTime.Today;
                    var todayRevenues = personalRev.Revenues
                        .Where(r => r.PaidAt.ToLocalTime().Date == todayLocal)
                        .ToList();

                    foreach (var rev in todayRevenues)
                    {
                        var hour = rev.PaidAt.ToLocalTime().Hour;
                        if (hour >= 0 && hour < 24)
                        {
                            HourlyRevenueData[hour] += rev.TotalAmount;
                        }
                    }
                }

                // 4. Fetch orders today
                var allOrders = await _orderService.GetAllOrdersAsync() ?? new();
                var myOrdersToday = allOrders
                    .Where(o => o.UserId == staffId.Value && o.OrderTime.ToLocalTime().Date == DateTime.Today)
                    .ToList();

                OrdersTodayCount = myOrdersToday.Count;

                // 5. Fetch paged sessions to track started/ended sessions today
                var pagedSessions = await _sessionService.GetPagedSessionsAsync(pageNumber: 1, pageSize: 1000);
                var mySessions = pagedSessions?.Items
                    .Where(s => s.UserId == staffId.Value)
                    .ToList() ?? new();

                SessionsHandledToday = mySessions
                    .Count(s => s.StartTime.ToLocalTime().Date == DateTime.Today || (s.EndTime.HasValue && s.EndTime.Value.ToLocalTime().Date == DateTime.Today));

                // 6. Build recent activities (Limit 8)
                var activities = new List<ActivityItem>();

                // Add session actions
                foreach (var s in mySessions)
                {
                    // Action: Session started
                    activities.Add(new ActivityItem
                    {
                        Action = $"Bắt đầu chơi bàn {s.TableName} ({s.DurationHours} giờ)",
                        Time = s.StartTime.ToLocalTime(),
                        Icon = "fa-play",
                        BadgeClass = "bg-success-subtle text-success border border-success"
                    });

                    // Action: Session ended
                    if (s.IsFinished && s.EndTime.HasValue)
                    {
                        activities.Add(new ActivityItem
                        {
                            Action = $"Kết thúc phiên chơi bàn {s.TableName}",
                            Time = s.EndTime.Value.ToLocalTime(),
                            Icon = "fa-stop",
                            BadgeClass = "bg-danger-subtle text-danger border border-danger"
                        });
                    }
                }

                // Add order actions
                foreach (var o in myOrdersToday)
                {
                    var sessionTableName = TableDashboard.FirstOrDefault(t => t.ActiveSession?.Id == o.SessionId)?.TableName 
                        ?? mySessions.FirstOrDefault(s => s.Id == o.SessionId)?.TableName 
                        ?? "Bàn chơi";

                    activities.Add(new ActivityItem
                    {
                        Action = $"Gọi dịch vụ cho {sessionTableName} (+{o.TotalAmount.ToString("N0")} đ)",
                        Time = o.OrderTime.ToLocalTime(),
                        Icon = "fa-cart-plus",
                        BadgeClass = "bg-info-subtle text-info border border-info"
                    });
                }

                RecentActivities = activities
                    .OrderByDescending(a => a.Time)
                    .Take(8)
                    .ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu bảng điều khiển: {ex.Message}";
            }

            return Page();
        }
    }
}
