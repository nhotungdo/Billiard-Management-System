using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RevenuesController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenuesController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("personal")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetPersonalRevenue([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid user token"));
                }

                var result = await _revenueService.GetPersonalRevenueAsync(userId, fromDate, toDate);
                return Ok(ApiResponse<PersonalRevenueDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("total")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] RevenueFilterQuery filter)
        {
            try
            {
                var result = await _revenueService.GetTotalRevenueAsync(filter);
                return Ok(ApiResponse<RevenueSummaryDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("staff/{staffId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueByStaff(Guid staffId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _revenueService.GetRevenueByStaffAsync(staffId, fromDate, toDate);
                return Ok(ApiResponse<PersonalRevenueDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
