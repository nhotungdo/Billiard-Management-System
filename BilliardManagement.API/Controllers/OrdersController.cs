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

            return Ok(ApiResponse<OrderDto>.Ok(order, "Order created successfully"));
        }
    }
}
