using Microsoft.AspNetCore.Mvc;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Pages.Admin.Categories
{
    public class IndexModel : AdminPageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public List<CategoryDto> Categories { get; set; } = new();

        [BindProperty]
        public CreateCategoryDto CategoryForm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _productService.GetAllCategoriesAsync();
            if (result != null)
            {
                Categories = result;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryForm.CategoryName))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống.";
                return RedirectToPage();
            }

            try
            {
                var result = await _productService.CreateCategoryAsync(CategoryForm);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Thêm danh mục thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể thêm danh mục.";
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id)
        {
            if (string.IsNullOrWhiteSpace(CategoryForm.CategoryName))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống.";
                return RedirectToPage();
            }

            try
            {
                var result = await _productService.UpdateCategoryAsync(id, CategoryForm);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật danh mục.";
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _productService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa danh mục thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa danh mục thất bại.";
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
