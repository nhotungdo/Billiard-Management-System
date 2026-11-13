using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Responses;
using BilliardManagement.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using BilliardManagement.API.Hubs;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly IHubContext<TableHub> _hubContext;
      
        public TablesController(ITableService tableService, IHubContext<TableHub> hubContext)
        {
            _tableService = tableService;
            _hubContext = hubContext;
        }
        // lấy danh sách bàn hoặc phân trang/tìm kiếm
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TableQueryParameters query)
        {
            var pagedResult = await _tableService.GetPagedTablesAsync(query);
            return Ok(ApiResponse<PagedResult<TableDto>>.Ok(pagedResult));
        }

        // L?y thng tin chi ti?t c?a m?t bn theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var table = await _tableService.GetTableByIdAsync(id);
            return Ok(ApiResponse<TableDto>.Ok(table));
        }

        // Tạo mới một bàn (Admin/SuperAdmin)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            var table = await _tableService.CreateTableAsync(dto);
            await _hubContext.Clients.Groups(StoreGroups.ForRequest(HttpContext)).SendAsync("ReceiveTableUpdate", $"Table {table.TableName} created.");
            return Ok(ApiResponse<TableDto>.Ok(table, "tạo bàn thành công"));
        }

        // Cập nhật thông tin bàn (Admin/SuperAdmin)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateTableDto dto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? updatedBy = Guid.TryParse(userIdStr, out var uId) ? uId : null;
            try
            {
                var table = await _tableService.UpdateTableAsync(id, dto, updatedBy);
                await _hubContext.Clients.Groups(StoreGroups.ForRequest(HttpContext)).SendAsync("ReceiveTableUpdate", $"Table {table.TableName} updated.");
                return Ok(ApiResponse<TableDto>.Ok(table, "Cập nhật bàn thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Xóa bàn (Admin/SuperAdmin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _tableService.DeleteTableAsync(id);
                if (!result) return NotFound(ApiResponse<object>.Fail("Không tìm thấy bàn để xóa"));
                await _hubContext.Clients.Groups(StoreGroups.ForRequest(HttpContext)).SendAsync("ReceiveTableUpdate", "Table deleted");
                return Ok(ApiResponse<bool>.Ok(true, "Xóa bàn thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Cập nhật trạng thái bàn (Admin, Staff, SuperAdmin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TableStatusUpdateDto request)
        {
            if (!Enum.TryParse<TableStatus>(request.Status, true, out var status))
            {
                return BadRequest(ApiResponse<object>.Fail("Trạng thái bàn không hợp lệ"));
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? updatedBy = Guid.TryParse(userIdStr, out var uId) ? uId : null;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            try
            {
                var table = await _tableService.UpdateTableStatusAsync(id, status, updatedBy, request.Reason, request.Force, isAdmin);

                var statusPayload = new TableStatusChangedDto
                {
                    TableId = table.Id,
                    TableName = table.TableName,
                    Status = (int)table.Status,
                    StatusName = table.Status.ToString()
                };
                await _hubContext.Clients.Groups(StoreGroups.ForRequest(HttpContext)).SendAsync("TableStatusChanged", statusPayload);
                await _hubContext.Clients.Groups(StoreGroups.ForRequest(HttpContext)).SendAsync("ReceiveTableUpdate", $"Table {table.TableName} status updated to {status}.");

                return Ok(ApiResponse<TableDto>.Ok(table, "Cập nhật trạng thái bàn thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Lấy lịch sử thay đổi trạng thái bàn
        [HttpGet("{id}/history")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            try
            {
                var history = await _tableService.GetTableHistoryAsync(id);
                return Ok(ApiResponse<IEnumerable<TableStatusHistoryDto>>.Ok(history));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
