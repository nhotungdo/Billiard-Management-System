using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilliardManagement.Web.Models;
using BilliardManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Combos
{
    public class IndexModel : AdminPageModel
    {
        private readonly ComboService _comboService;
        private readonly ProductService _productService;

        public IndexModel(ComboService comboService, ProductService productService)
        {
            _comboService = comboService;
            _productService = productService;
        }

        public List<ComboDto> Combos { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? ActiveOnly { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var combosResult = await _comboService.GetAllCombosAsync(Search, ActiveOnly);
                Combos = combosResult ?? new();

                var productsResult = await _productService.GetAllProductsAsync();
                Products = productsResult?.Where(p => p.IsAvailable).ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] CreateComboDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.ComboCode) || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng nhập đầy đủ Mã và Tên Combo." });
                }

                var created = await _comboService.CreateComboAsync(dto);
                return new JsonResult(new { success = true, message = $"Tạo gói combo '{dto.Name}' thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id, [FromBody] CreateComboDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.ComboCode) || string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                var success = await _comboService.UpdateComboAsync(id, dto);
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Cập nhật gói combo thất bại." });
                }

                return new JsonResult(new { success = true, message = "Cập nhật gói combo thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            try
            {
                var success = await _comboService.ToggleComboStatusAsync(id);
                if (success)
                {
                    SuccessMessage = "Đã thay đổi trạng thái gói combo!";
                }
                else
                {
                    ErrorMessage = "Không thể đổi trạng thái combo.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _comboService.DeleteComboAsync(id);
                if (success)
                {
                    SuccessMessage = "Đã xóa gói combo thành công!";
                }
                else
                {
                    ErrorMessage = "Không tìm thấy gói combo để xóa.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
