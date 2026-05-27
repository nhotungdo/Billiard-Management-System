using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Pages.Staff.Sessions
{
    public class IndexModel : StaffPageModel
    {
        private readonly SessionService _sessionService;
        private readonly ProductService _productService;

        public IndexModel(
            SessionService sessionService,
            ProductService productService)
        {
            _sessionService = sessionService;
            _productService = productService;
        }

        public List<TableDashboardDto> TableDashboard { get; set; } = new();
        public List<SessionDto> ActiveSessions { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int CountAvailable => TableDashboard.Count(t => t.Status == 1);
        public int CountPlaying => TableDashboard.Count(t => t.Status == 2);
        public int CountReserved => TableDashboard.Count(t => t.Status == 3);
        public int CountMaintenance => TableDashboard.Count(t => t.Status == 4);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var dashboard = await _sessionService.GetDashboardAsync();
                TableDashboard = dashboard?.OrderBy(d => d.TableName).ToList() ?? new();

                ActiveSessions = TableDashboard
                    .Where(t => t.ActiveSession != null)
                    .Select(t => t.ActiveSession!)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                var products = await _productService.GetAllProductsAsync();
                Products = products?.Where(p => p.Stock > 0 && p.IsAvailable).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
            }

            return Page();
        }

        public static int GetRemainingSeconds(SessionDto s)
        {
            return s.RemainingSeconds;
        }

        public static DateTime GetEndUtc(SessionDto s)
        {
            return s.EndTime ?? s.StartTime.AddHours(s.DurationHours);
        }

        public static string FormatTimer(int s)
        {
            var sec = Math.Max(0, s);
            var h = sec / 3600;
            var m = (sec % 3600) / 60;
            var r = sec % 60;
            return $"{h:D2}:{m:D2}:{r:D2}";
        }
    }
}
