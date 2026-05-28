using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Pages.Staff.Tables
{
    public class MapModel : StaffPageModel
    {
        private readonly SessionService _sessionService;
        private readonly ProductService _productService;

        public MapModel(SessionService sessionService, ProductService productService)
        {
            _sessionService = sessionService;
            _productService = productService;
        }

        public List<TableDashboardDto> TableDashboard { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int CountAvailable => TableDashboard.Count(t => t.Status == 1);
        public int CountPlaying => TableDashboard.Count(t => t.Status == 2);
        public int CountReserved => TableDashboard.Count(t => t.Status == 3);
        public int CountMaintenance => TableDashboard.Count(t => t.Status == 4);

        public decimal ActiveSessionsRevenue => TableDashboard
            .Where(t => t.Status == 2 && t.ActiveSession != null)
            .Sum(t => t.ActiveSession!.CurrentTotal);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var dashboard = await _sessionService.GetDashboardAsync();
                TableDashboard = dashboard?.OrderBy(d => d.TableName).ToList() ?? new();

                var products = await _productService.GetAllProductsAsync();
                Products = products?.Where(p => p.Stock > 0 && p.IsAvailable)
                                    .OrderBy(p => p.Category)
                                    .ThenBy(p => p.Name)
                                    .ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể tải dữ liệu sơ đồ bàn: {ex.Message}";
            }

            return Page();
        }
    }
}
