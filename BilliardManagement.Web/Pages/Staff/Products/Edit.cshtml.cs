using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BilliardManagement.Web.Pages.Staff.Products
{
    public class EditModel : AdminOrStaffPageModel
    {
        private readonly ProductService _productService;
        public EditModel(ProductService productService) { _productService = productService; }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public EditProductViewModel EditProduct { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ExistingImageUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(Id);
                if (product == null)
                {
                    return RedirectToPage("./Index");
                }

                EditProduct = new EditProductViewModel
                {
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    Stock = product.Stock,
                    Description = product.Description,
                    IsAvailable = product.IsAvailable,
                    ExistingImageUrl = product.ImageUrl
                };
                ExistingImageUrl = product.ImageUrl;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var success = await _productService.UpdateProductWithImageAsync(
                    Id,
                    EditProduct.Name,
                    EditProduct.Category,
                    EditProduct.Price,
                    EditProduct.Description,
                    EditProduct.IsAvailable,
                    EditProduct.Stock,
                    EditProduct.ExistingImageUrl,
                    EditProduct.ImageFile
                );

                if (success)
                {
                    TempData["SuccessMessage"] = "Cáº­p nháº­t sáº£n pháº©m thÃ nh cÃ´ng";
                    return RedirectToPage("./Index");
                }
                
                ErrorMessage = "KhÃ´ng thá»ƒ cáº­p nháº­t sáº£n pháº©m";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ExistingImageUrl = EditProduct.ExistingImageUrl;
            }

            return Page();
        }
    }

    public class EditProductViewModel
    {
        [Required(ErrorMessage = "TÃªn sáº£n pháº©m khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh má»¥c khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "GiÃ¡ bÃ¡n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        [Range(1000, double.MaxValue, ErrorMessage = "GiÃ¡ bÃ¡n pháº£i lá»›n hÆ¡n 0")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        public string? ExistingImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}

