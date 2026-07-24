using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CombosController : ControllerBase
    {
        private readonly IComboService _comboService;
        private readonly ILogger<CombosController> _logger;

        public CombosController(IComboService comboService, ILogger<CombosController> logger)
        {
            _comboService = comboService;
            _logger = logger;
        }

        // GET: api/combos
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? activeOnly)
        {
            var combos = await _comboService.GetAllCombosAsync(search, activeOnly);
            return Ok(ApiResponse<IEnumerable<ComboDto>>.Ok(combos));
        }

        // GET: api/combos/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var combo = await _comboService.GetComboByIdAsync(id);
            if (combo == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói combo"));

            return Ok(ApiResponse<ComboDto>.Ok(combo));
        }

        // GET: api/combos/code/{code}
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var combo = await _comboService.GetComboByCodeAsync(code);
            if (combo == null)
                return NotFound(ApiResponse<object>.Fail($"Không tìm thấy combo với mã '{code}'"));

            return Ok(ApiResponse<ComboDto>.Ok(combo));
        }

        // POST: api/combos
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateComboDto dto)
        {
            try
            {
                var combo = await _comboService.CreateComboAsync(dto);
                return Ok(ApiResponse<ComboDto>.Ok(combo, "Tạo gói combo thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Create combo failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // PUT: api/combos/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComboDto dto)
        {
            try
            {
                var combo = await _comboService.UpdateComboAsync(id, dto);
                if (combo == null)
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói combo để cập nhật"));

                return Ok(ApiResponse<ComboDto>.Ok(combo, "Cập nhật gói combo thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update combo failed for {ComboId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // PATCH: api/combos/{id}/toggle-status
        [HttpPatch("{id:guid}/toggle-status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var success = await _comboService.ToggleComboStatusAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói combo"));

            return Ok(ApiResponse<object?>.Ok(null, "Cập nhật trạng thái combo thành công"));
        }

        // DELETE: api/combos/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _comboService.DeleteComboAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy gói combo"));

            return Ok(ApiResponse<object?>.Ok(null, "Xóa gói combo thành công"));
        }

        // POST: api/combos/apply
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyCombo([FromBody] ApplyComboRequestDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail("Tài khoản người dùng không hợp lệ"));
            }

            var result = await _comboService.ApplyComboToSessionAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<ApplyComboResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<ApplyComboResultDto>.Ok(result, result.Message));
        }
    }
}
