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
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }
        // nhân viên bắt đầu ca làm việc
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var shift = await _shiftService.CheckInAsync(userId);
            return Ok(ApiResponse<ShiftDto>.Ok(shift, "Bắt đầu ca làm việc thành công"));
        }
        // nhân viên kết thúc ca làm việc
        [HttpPost("checkout/{shiftId}")]
        public async Task<IActionResult> CheckOut(Guid shiftId)
        {
            var shift = await _shiftService.CheckOutAsync(shiftId);
            return Ok(ApiResponse<ShiftDto>.Ok(shift, "Kết thúc ca làm việc thành công"));
        }
        // admin xem tất cả ca làm việc trong ngày
        [HttpGet("today")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTodayShifts()
        {
            var shifts = await _shiftService.GetTodayShiftsAsync();
            return Ok(ApiResponse<IEnumerable<ShiftDto>>.Ok(shifts));
        }


     


    }
}
