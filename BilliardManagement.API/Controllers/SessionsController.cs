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

        public SessionsController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("start/{tableId}")]
        public async Task<IActionResult> StartSession(Guid tableId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var session = await _sessionService.StartSessionAsync(tableId, userId);
            return Ok(ApiResponse<SessionDto>.Ok(session, "Session started"));
        }

        [HttpPost("end/{sessionId}")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            var session = await _sessionService.EndSessionAsync(sessionId);
            return Ok(ApiResponse<SessionDto>.Ok(session, "Session ended"));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _sessionService.GetActiveSessionsAsync();
            return Ok(ApiResponse<IEnumerable<SessionDto>>.Ok(sessions));
        }
    }
}
