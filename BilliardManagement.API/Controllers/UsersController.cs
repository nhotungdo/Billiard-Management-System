using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;
using BilliardManagement.Business.Services;
using System.Security.Claims;

namespace BilliardManagement.API.Controllers
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService, 
            IWebHostEnvironment env, 
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _env = env;
            _logger = logger;
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
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse<UserDeletionResultDto>.Ok(result, "User deleted successfully"));
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng"));
            var user = await _userService.GetUserByIdAsync(userId.Value);
            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng"));

                string? imageUrl = null;
                if (request.ProfilePicture != null && request.ProfilePicture.Length > 0)
                {
                    if (!ProductService.IsValidImageExtension(request.ProfilePicture.FileName))
                        return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận ảnh JPG, JPEG, PNG"));

                    var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "avatars");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(request.ProfilePicture.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ProfilePicture.CopyToAsync(stream);
                    }

                    imageUrl = $"/uploads/avatars/{fileName}";
                }

                var dto = new UpdateProfileDto
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    ProfilePictureUrl = imageUrl
                };

                var user = await _userService.UpdateProfileAsync(userId.Value, dto);
                return Ok(ApiResponse<UserDto>.Ok(user, "Cập nhật trang cá nhân thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật profile");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // PUT: api/users/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng"));

                var success = await _userService.ChangePasswordAsync(userId.Value, dto);
                return Ok(ApiResponse<bool>.Ok(success, "Đổi mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đổi mật khẩu");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // POST: api/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordForUserDto dto)
        {
            try
            {
                var adminUsername = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";

                if (string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    return BadRequest(ApiResponse<object>.Fail("Mật khẩu mới không được để trống."));
                }

                var newPassword = await _userService.ResetUserPasswordAsync(id, dto.NewPassword);
                
                _logger.LogInformation("Admin user {AdminUsername} reset password for user ID {UserId} at {Time}", 
                    adminUsername, id, DateTime.UtcNow);

                return Ok(ApiResponse<string>.Ok(newPassword, "Mật khẩu đã được đặt lại thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi reset mật khẩu cho user ID {UserId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
