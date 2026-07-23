using System.Security.Claims;
using BilliardManagement.API.Hubs;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BilliardManagement.API.Controllers
{
    [Route("api/table-sessions")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class TableSessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ITableService _tableService;
        private readonly IHubContext<TableHub> _hubContext;
        private readonly ILogger<TableSessionsController> _logger;

        public TableSessionsController(
            ISessionService sessionService,
            ITableService tableService,
            IHubContext<TableHub> hubContext,
            ILogger<TableSessionsController> logger)
        {
            _sessionService = sessionService;
            _tableService = tableService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _sessionService.GetTableDashboardAsync();
            return Ok(ApiResponse<IEnumerable<TableDashboardDto>>.Ok(dashboard));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var sessions = await _sessionService.GetActiveSessionsAsync();
            return Ok(ApiResponse<IEnumerable<SessionDto>>.Ok(sessions));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SessionQueryParameters query)
        {
            try
            {
                var pagedResult = await _sessionService.GetPagedSessionsAsync(query);
                return Ok(ApiResponse<PagedResult<SessionDto>>.Ok(pagedResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lấy danh sách các phiên chơi lỗi: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartSessionRequest request)
        {
            try
            {
                if (request == null || request.TableId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.Fail("TableId is required"));

                if (string.IsNullOrWhiteSpace(request.CustomerPhone) || !System.Text.RegularExpressions.Regex.IsMatch(request.CustomerPhone.Trim(), @"^0\d{9}$"))
                    return BadRequest(ApiResponse<object>.Fail("Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678)."));

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));

                var session = await _sessionService.StartSessionAsync(request.TableId, userId.Value, request.DurationHours, request.CustomerName, request.CustomerPhone, request.PaymentMethod);
                await BroadcastSessionAsync(session, "SessionStarted");
                return Ok(ApiResponse<SessionDto>.Ok(session, "Bắt đầu phiên chơi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Start session failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("extend/{id}")]
        public async Task<IActionResult> Extend(Guid id, [FromBody] ExtendSessionRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.Fail("Request body is required"));

                var userId = GetUserId();
                var session = await _sessionService.ExtendSessionAsync(id, request.AdditionalMinutes, userId);
                await BroadcastSessionAsync(session, "SessionExtended");
                return Ok(ApiResponse<SessionDto>.Ok(session, "Gia hạn phiên chơi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Extend session failed for {SessionId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("end/{id}")]
        public async Task<IActionResult> End(Guid id, [FromBody] EndSessionRequest? request)
        {
            try
            {
                var userId = GetUserId();
                GenerateBillDto? billDto = null;
                if (request != null)
                {
                    billDto = new GenerateBillDto
                    {
                        PaymentMethod = request.PaymentMethod
                    };
                }

                var session = await _sessionService.EndSessionAsync(id, billDto, userId);
                await BroadcastSessionAsync(session, "SessionEnded");
                return Ok(ApiResponse<SessionDto>.Ok(session, "Kết thúc phiên chơi và tạo hóa đơn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "End session failed for {SessionId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }

        private async Task BroadcastSessionAsync(SessionDto session, string eventName)
        {
            var table = await _tableService.GetTableByIdAsync(session.TableId);

            var statusPayload = new TableStatusChangedDto
            {
                TableId = table.Id,
                TableName = table.TableName,
                Status = (int)table.Status,
                StatusName = table.Status.ToString()
            };
            await _hubContext.Clients.All.SendAsync("TableStatusChanged", statusPayload);

            var realtime = new SessionRealtimeDto
            {
                SessionId = session.Id,
                TableId = session.TableId,
                TableName = session.TableName ?? table.TableName,
                Status = (int)session.Status,
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
                CustomerName = session.CustomerName,
                CustomerPhone = session.CustomerPhone,
                OrderLines = session.OrderLines
            };

            await _hubContext.Clients.All.SendAsync("SessionUpdated", realtime);
            await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", eventName);
        }
    }
}
