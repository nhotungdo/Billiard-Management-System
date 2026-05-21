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
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ITableService _tableService;
        private readonly IHubContext<TableHub> _hubContext;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(
            ISessionService sessionService,
            ITableService tableService,
            IHubContext<TableHub> hubContext,
            ILogger<SessionsController> logger)
        {
            _sessionService = sessionService;
            _tableService = tableService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("start/{tableId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> StartSession(Guid tableId, [FromQuery] int durationHours = 1)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

                var session = await _sessionService.StartSessionAsync(tableId, userId.Value, durationHours);
                await BroadcastAsync(session, "Session started");
                return Ok(ApiResponse<SessionDto>.Ok(session, "Session started successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartSession failed for tableId={TableId}", tableId);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("end/{sessionId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            try
            {
                var session = await _sessionService.EndSessionAsync(sessionId, null, GetUserId());
                await BroadcastAsync(session, "Session ended");
                return Ok(ApiResponse<SessionDto>.Ok(session, "Session ended successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EndSession failed for sessionId={SessionId}", sessionId);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync();
                return Ok(ApiResponse<IEnumerable<SessionDto>>.Ok(sessions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActiveSessions failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }

        private async Task BroadcastAsync(SessionDto session, string message)
        {
            var table = await _tableService.GetTableByIdAsync(session.TableId);
            var payload = new TableStatusChangedDto
            {
                TableId = table.Id,
                TableName = table.TableName,
                Status = (int)table.Status,
                StatusName = table.Status.ToString()
            };
            await _hubContext.Clients.All.SendAsync("TableStatusChanged", payload);
            await _hubContext.Clients.All.SendAsync("SessionUpdated", new SessionRealtimeDto
            {
                SessionId = session.Id,
                TableId = session.TableId,
                TableName = session.TableName ?? table.TableName,
                TableStatus = (int)table.Status,
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
            });
            await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", message);
        }
    }
}
