using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var stores = await _storeService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<StoreDto>>.Ok(stores));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var store = await _storeService.GetByIdAsync(id);
            return Ok(ApiResponse<StoreDto>.Ok(store));
        }

        [HttpGet("current")]
        [Authorize(Roles = "Admin,Staff,SuperAdmin")]
        public async Task<IActionResult> Current([FromServices] BilliardManagement.Common.Tenant.ITenantContext tenant)
        {
            if (tenant.StoreId == Guid.Empty) return NotFound(ApiResponse<object>.Fail("Chưa chọn cửa hàng"));
            return Ok(ApiResponse<StoreDto>.Ok(await _storeService.GetByIdAsync(tenant.StoreId)));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateStoreDto dto)
        {
            var store = await _storeService.CreateAsync(dto);
            return Ok(ApiResponse<StoreDto>.Ok(store, "Tạo cửa hàng thành công"));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto)
        {
            var store = await _storeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<StoreDto>.Ok(store, "Cập nhật cửa hàng thành công"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _storeService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy cửa hàng"));

            return Ok(ApiResponse<object?>.Ok(null, "Vô hiệu hóa cửa hàng thành công"));
        }
    }
}
