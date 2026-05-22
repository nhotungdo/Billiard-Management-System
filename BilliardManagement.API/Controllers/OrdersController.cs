using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using BilliardManagement.API.Hubs;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrdersController(IOrderService orderService, IHubContext<OrderHub> hubContext)
        {
            _orderService = orderService;
            _hubContext = hubContext;
        }

        // tạo hóa đơn mới
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var order = await _orderService.CreateOrderAsync(dto, userId);
            
            // Notify Kitchen/Bar
            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", $"New order {order.Id} created for session {dto.SessionId}");

            return Ok(ApiResponse<OrderDto>.Ok(order, "Đơn hàng đã được tạo thành công"));
        }
        
        // Lấy tất cả hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders, "Đơn hàng đã được lấy thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Đơn hàng đã được lấy thành công"));
        }
    }
}
