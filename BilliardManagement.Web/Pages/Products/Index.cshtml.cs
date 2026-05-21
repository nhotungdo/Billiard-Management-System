using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public List<ProductDto> Products { get; set; } = new();

        [BindProperty]
        public CreateProductDto NewProduct { get; set; } = new();

        [BindProperty]
        public CreateProductDto EditProduct { get; set; } = new();

        [BindProperty]
        public Guid EditId { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var list = await _productService.GetAllProductsAsync();
                if (list != null)
                {
                    Products = list;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load products: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return Page();
            }

            try
            {
                var result = await _productService.CreateProductAsync(NewProduct);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Product created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create product.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating product: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return Page();
            }

            try
            {
                var success = await _productService.UpdateProductAsync(EditId, EditProduct);
                if (success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update product.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating product: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete product.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
