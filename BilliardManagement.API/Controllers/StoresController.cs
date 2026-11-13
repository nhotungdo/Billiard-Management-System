using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stores = await _storeService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<StoreDto>>.Ok(stores));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var store = await _storeService.GetByIdAsync(id);
            return Ok(ApiResponse<StoreDto>.Ok(store));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStoreDto dto)
        {
            var store = await _storeService.CreateAsync(dto);
            return Ok(ApiResponse<StoreDto>.Ok(store, "Tạo cửa hàng thành công"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto)
        {
            var store = await _storeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<StoreDto>.Ok(store, "Cập nhật cửa hàng thành công"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _storeService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy cửa hàng"));

            return Ok(ApiResponse<object?>.Ok(null, "Vô hiệu hóa cửa hàng thành công"));
        }
    }
}
