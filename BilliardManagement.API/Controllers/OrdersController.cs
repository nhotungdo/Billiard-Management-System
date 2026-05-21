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
    [Authorize(Roles = "Admin,Staff")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISessionService _sessionService;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly IHubContext<TableHub> _tableHub;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ISessionService sessionService,
            IHubContext<OrderHub> orderHub,
            IHubContext<TableHub> tableHub,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _orderHub = orderHub;
            _tableHub = tableHub;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized();

                var sessionId = dto.SessionId != Guid.Empty ? dto.SessionId : dto.TableSessionId;
                var order = await _orderService.CreateOrderAsync(dto, userId);

                _logger.LogInformation("Order created: orderId={OrderId}, sessionId={SessionId}", order.Id, sessionId);

                await _orderHub.Clients.All.SendAsync("ReceiveOrderUpdate", $"New order {order.Id} for session {sessionId}");

                var sessions = await _sessionService.GetActiveSessionsAsync();
                var session = sessions.FirstOrDefault(s => s.Id == sessionId);
                if (session != null)
                    await BroadcastSessionAsync(session);

                return Ok(ApiResponse<OrderDto>.Ok(order, "Order thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CreateOrder failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }

        private async Task BroadcastSessionAsync(SessionDto session)
        {
            var realtime = new SessionRealtimeDto
            {
                SessionId = session.Id,
                TableId = session.TableId,
                TableName = session.TableName ?? "",
                Status = (int)session.Status,
                TableStatus = 2,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                DurationHours = session.DurationHours,
                RemainingMinutes = session.RemainingMinutes,
                RemainingSeconds = session.RemainingSeconds,
                TotalPrice = session.TotalPrice,
                OrdersTotal = session.OrdersTotal,
                CurrentTotal = session.CurrentTotal,
                IsExpired = session.IsExpired,
                IsFinished = session.IsFinished,
                TimerLevel = session.IsExpired ? "expired" : session.RemainingMinutes < 15 ? "warning" : "ok",
                OrderLines = session.OrderLines
            };

            await _tableHub.Clients.All.SendAsync("SessionUpdated", realtime);
        }
    }
}
