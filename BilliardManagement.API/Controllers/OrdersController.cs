using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using BilliardManagement.API.Hubs;
using BilliardManagement.Models.Enums;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BilliardManagement.API.Controllers
{
    public class UpdateOrderStatusRequest
    {
        public int Status { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISessionService _sessionService;
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly IHubContext<TableHub> _tableHubContext;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ISessionService sessionService,
            IHubContext<OrderHub> orderHubContext,
            IHubContext<TableHub> tableHubContext,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _orderHubContext = orderHubContext;
            _tableHubContext = tableHubContext;
            _logger = logger;
        }

        // tạo hóa đơn mới
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var order = await _orderService.CreateOrderAsync(dto, userId);
            
            // Notify Kitchen/Bar
            await _orderHubContext.Clients.All.SendAsync("ReceiveOrderUpdate", $"New order {order.Id} created for session {dto.SessionId}");

            return Ok(ApiResponse<OrderDto>.Ok(order, "Đơn hàng đã được tạo thành công"));
        }
        
        // Lấy tất cả đơn hàng hoặc phân trang/tìm kiếm
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderQueryParameters query)
        {
            var pagedResult = await _orderService.GetPagedOrdersAsync(query);
            return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(pagedResult, "Đơn hàng đã được lấy thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Đơn hàng đã được lấy thành công"));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var newStatus = (OrderStatus)request.Status;
                var order = await _orderService.UpdateOrderStatusAsync(id, newStatus);

                _logger.LogInformation("Order {OrderId} status updated to {Status}", id, newStatus);

                // Notify OrderHub (kitchen/bar views)
                await _orderHubContext.Clients.All.SendAsync("ReceiveOrderUpdate", $"Order {id} state changed to {newStatus}");

                // Notify TableHub for active session updates on dashboard
                if (order.SessionId != Guid.Empty)
                {
                    try
                    {
                        var activeSessions = await _sessionService.GetActiveSessionsAsync();
                        var sessionDto = activeSessions.FirstOrDefault(s => s.Id == order.SessionId);
                        if (sessionDto != null)
                        {
                            var realtime = new SessionRealtimeDto
                            {
                                SessionId = sessionDto.Id,
                                TableId = sessionDto.TableId,
                                TableName = sessionDto.TableName ?? string.Empty,
                                Status = (int)sessionDto.Status,
                                TableStatus = 2, // Playing
                                StartTime = sessionDto.StartTime,
                                EndTime = sessionDto.EndTime,
                                DurationHours = sessionDto.DurationHours,
                                RemainingMinutes = sessionDto.RemainingMinutes,
                                RemainingSeconds = sessionDto.RemainingSeconds,
                                TotalPrice = sessionDto.TotalPrice,
                                OrdersTotal = sessionDto.OrdersTotal,
                                CurrentTotal = sessionDto.CurrentTotal,
                                IsExpired = sessionDto.IsExpired,
                                IsFinished = sessionDto.IsFinished,
                                TimerLevel = sessionDto.IsExpired ? "expired" : sessionDto.RemainingMinutes < 15 ? "warning" : "ok",
                                OrderLines = sessionDto.OrderLines
                            };
                            await _tableHubContext.Clients.All.SendAsync("SessionUpdated", realtime);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to broadcast session update on order status change");
                    }

                    await _tableHubContext.Clients.All.SendAsync("ReceiveTableUpdate", "OrderCompleted");
                }

                return Ok(ApiResponse<OrderDto>.Ok(order, "Cập nhật trạng thái đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update order status failed for {OrderId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
