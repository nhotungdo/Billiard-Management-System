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

        public int ActiveCount => Combos.Count(c => c.IsActive);
        public int InactiveCount => Combos.Count(c => !c.IsActive);
        public decimal AveragePrice => Combos.Count == 0 ? 0 : Combos.Average(c => c.Price);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Search = Search?.Trim();
                var combosResult = await _comboService.GetAllCombosAsync(Search, ActiveOnly);
                Combos = combosResult ?? new();

                var productsResult = await _productService.GetAllProductsAsync();
                Products = productsResult?.Where(p => p.IsAvailable).ToList() ?? new();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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

                await _comboService.CreateComboAsync(dto);
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

                return new JsonResult(new { success = true, message = $"Cập nhật gói combo '{dto.Name}' thành công!" });
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
                    TempData["SuccessMessage"] = "Đã thay đổi trạng thái gói combo!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể đổi trạng thái gói combo.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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
                    TempData["SuccessMessage"] = "Đã xóa gói combo thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy gói combo để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
