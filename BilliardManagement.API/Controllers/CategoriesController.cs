using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(categories));
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(ApiResponse<CategoryDto>.Ok(category));
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(dto);
                return Ok(ApiResponse<CategoryDto>.Ok(category, "Tạo danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Create category failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                return Ok(ApiResponse<CategoryDto>.Ok(category, "Cập nhật danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update category failed for {CategoryId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Không tìm thấy danh mục"));

                return Ok(ApiResponse<object?>.Ok(null, "Xóa danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete category failed for {CategoryId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
