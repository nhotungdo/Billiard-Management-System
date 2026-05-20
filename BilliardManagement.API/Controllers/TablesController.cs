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
        // l?y danh sách bàn
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _tableService.GetAllTablesAsync();
            return Ok(ApiResponse<IEnumerable<TableDto>>.Ok(tables));
        }

        // L?y thông tin chi ti?t c?a m?t bàn theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var table = await _tableService.GetTableByIdAsync(id);
            return Ok(ApiResponse<TableDto>.Ok(table));
        }

        // T?o m?i m?t bàn (ch? dành cho Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            var table = await _tableService.CreateTableAsync(dto);
            await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", $"Table {table.TableName} created.");
            return Ok(ApiResponse<TableDto>.Ok(table, "Table created successfully"));
        }

        // C?p nh?t thông tin bàn (ch? dành cho Admin)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TableStatus status)
        {
            var table = await _tableService.UpdateTableStatusAsync(id, status);
            await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", $"Table {table.TableName} status updated to {status}.");
            return Ok(ApiResponse<TableDto>.Ok(table, "Status updated successfully"));
        }
    }
}
