using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _reportService.GetDashboardAnalyticsAsync();
            return Ok(ApiResponse<object>.Ok(data));
        }

        [HttpGet("revenue/daily")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] int days = 7)
        {
            var data = await _reportService.GetDailyRevenueAsync(days);
            return Ok(ApiResponse<object>.Ok(data));
        }
    }
}
