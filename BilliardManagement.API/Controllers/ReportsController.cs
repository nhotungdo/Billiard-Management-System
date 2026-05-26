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

        // dashboard admin 
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _reportService.GetDashboardAnalyticsAsync();
            return Ok(ApiResponse<object>.Ok(data));
        }

        // báo cáo doanh thu hàng ngày trong 7 ngày gần nhất
        [HttpGet("revenue/daily")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] int days = 7)
        {
            var data = await _reportService.GetDailyRevenueAsync(days);
            return Ok(ApiResponse<object>.Ok(data));
        }

        // Báo cáo doanh thu theo khoảng thời gian và nhóm (ngày, tháng, năm)
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] string groupType = "day")
        {
            var start = startDate ?? DateTime.UtcNow.Date.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;
            var data = await _reportService.GetRevenueReportAsync(start, end, groupType);
            return Ok(ApiResponse<object>.Ok(data));
        }
    }
}
