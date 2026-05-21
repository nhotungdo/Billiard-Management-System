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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            try
            {
                var userId = GetUserId();
                var table = await _tableService.CreateTableAsync(dto, userId);

                _logger.LogInformation(
                    "User {UserId} created table {TableId} ({TableName}) at {Time}",
                    userId, table.Id, table.TableName, DateTime.UtcNow);

                var payload = new TableCreatedDto
                {
                    Id = table.Id,
                    TableName = table.TableName,
                    TableType = table.TableType,
                    Status = (int)table.Status,
                    PricePerHour = table.PricePerHour,
                    Description = table.Description
                };

                await _hubContext.Clients.All.SendAsync("TableCreated", payload);
                await BroadcastTableStatusAsync(table, "Table created");

                return Ok(ApiResponse<TableDto>.Ok(table, "T?o b�n m?i th�nh c�ng"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Create table failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
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

            Guid? updatedBy = GetUserId();

            try
            {
                var table = await _tableService.UpdateTableStatusAsync(id, status, updatedBy);
                await BroadcastTableStatusAsync(table, "Status updated");
                return Ok(ApiResponse<TableDto>.Ok(table, "C?p nh?t tr?ng th�i b�n th�nh c�ng"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UpdateStatus failed for table {TableId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateTableDto dto)
        {
            try
            {
                var userId = GetUserId();
                var table = await _tableService.UpdateTableAsync(id, dto, userId);

                _logger.LogInformation(
                    "User {UserId} updated table {TableId} ({TableName}) at {Time}",
                    userId, table.Id, table.TableName, DateTime.UtcNow);

                await BroadcastTableStatusAsync(table, "Table updated");

                return Ok(ApiResponse<TableDto>.Ok(table, "Cáº­p nháº­t bÃ n thÃ nh cÃ´ng"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update table failed for {TableId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _tableService.DeleteTableAsync(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Table not found"));

                _logger.LogInformation("Table {TableId} deleted at {Time}", id, DateTime.UtcNow);
                await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", "Table deleted");

                return Ok(ApiResponse<object>.Ok(null, "XÃ³a bÃ n thÃ nh cÃ´ng"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete table failed for {TableId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
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
