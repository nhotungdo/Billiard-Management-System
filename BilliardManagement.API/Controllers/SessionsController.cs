using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using System.Security.Claims;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        // Bắt đầu phiên chơi cho một bàn
        [HttpPost("start/{tableId}")]
        public async Task<IActionResult> StartSession(Guid tableId, [FromQuery] int durationHours = 2)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("bắt đầu phiên chơi: tableId={TableId}, userId={UserId}", tableId, userIdStr);

                if (!Guid.TryParse(userIdStr, out var userId))
                {
                    _logger.LogWarning("bắt đầu phiên chơi lỗi: không tìm thấy user id");
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized: invalid user token"));
                }

                var session = await _sessionService.StartSessionAsync(tableId, userId, durationHours);
                _logger.LogInformation("bắt đầu phiên chơi thành công: sessionId={SessionId}", session.Id);
                return Ok(ApiResponse<SessionDto>.Ok(session, "bắt đầu phiên chơi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "bắt đầu phiên chơi lỗi: tableId={TableId}: {Message}", tableId, ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Kết thúc phiên chơi
        [HttpPost("end/{sessionId}")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            try
            {
                _logger.LogInformation("kết thúc phiên chơi: sessionId={SessionId}", sessionId);

                var session = await _sessionService.EndSessionAsync(sessionId);
                _logger.LogInformation("kết thúc phiên chơi thành công: sessionId={SessionId}, totalPrice={TotalPrice}", session.Id, session.TotalPrice);
                return Ok(ApiResponse<SessionDto>.Ok(session, "kết thúc phiên chơi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "kết thúc phiên chơi lỗi: sessionId={SessionId}: {Message}", sessionId, ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Lấy danh sách các phiên chơi đang hoạt động
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
                _logger.LogError(ex, "lấy danh sách các phiên chơi đang hoạt động lỗi: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Lấy danh sách phiên chơi hoặc phân trang/tìm kiếm
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
    }
}
