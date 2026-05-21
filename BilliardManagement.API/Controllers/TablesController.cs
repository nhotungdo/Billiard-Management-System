using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Responses;
using BilliardManagement.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using BilliardManagement.API.Hubs;
using System.Security.Claims;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly IHubContext<TableHub> _hubContext;
        private readonly ILogger<TablesController> _logger;

        public TablesController(ITableService tableService, IHubContext<TableHub> hubContext, ILogger<TablesController> logger)
        {
            _tableService = tableService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _tableService.GetAllTablesAsync();
            return Ok(ApiResponse<IEnumerable<TableDto>>.Ok(tables));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var table = await _tableService.GetTableByIdAsync(id);
            return Ok(ApiResponse<TableDto>.Ok(table));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            var table = await _tableService.CreateTableAsync(dto);
            await BroadcastTableStatusAsync(table, "Table created");
            return Ok(ApiResponse<TableDto>.Ok(table, "Table created successfully"));
        }

        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTableStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(ApiResponse<object>.Fail("Status is required"));

            if (!Enum.TryParse<TableStatus>(request.Status, true, out var status) || !Enum.IsDefined(typeof(TableStatus), status))
                return BadRequest(ApiResponse<object>.Fail("Invalid table status"));

            var isStaff = User.IsInRole("Staff") && !User.IsInRole("Admin");
            if (isStaff && status == TableStatus.Maintenance)
                return Forbid();

            if (isStaff && status != TableStatus.Available && status != TableStatus.Playing && status != TableStatus.Reserved)
                return BadRequest(ApiResponse<object>.Fail("Staff can only set Available, Playing, or Reserved"));

            Guid? updatedBy = null;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                updatedBy = userId;

            try
            {
                var table = await _tableService.UpdateTableStatusAsync(id, status, updatedBy);
                await BroadcastTableStatusAsync(table, "Status updated");
                return Ok(ApiResponse<TableDto>.Ok(table, "C?p nh?t tr?ng thùi bùn thùnh cùng"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UpdateStatus failed for table {TableId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        private async Task BroadcastTableStatusAsync(TableDto table, string message)
        {
            var payload = new TableStatusChangedDto
            {
                TableId = table.Id,
                TableName = table.TableName,
                Status = (int)table.Status,
                StatusName = table.Status.ToString()
            };

            await _hubContext.Clients.All.SendAsync("TableStatusChanged", payload);
            await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", message);
        }
    }
}
