using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        // lấy tất cả người dùng (chỉ dành cho admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(users));
        }

        // lấy thông tin người dùng theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        // cập nhật vai trò người dùng (chỉ dành cho admin)
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UserRole role)
        {
            var user = await _userService.UpdateUserRoleAsync(id, role);
            return Ok(ApiResponse<UserDto>.Ok(user, "Role updated successfully"));
        }

        // Xóa 1 nhân viên (id) - trả về số khách hàng đã phục vụ và số mặt hàng đã bán 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse<UserDeletionResultDto>.Ok(result, "User deleted successfully"));

        }
        }
}
