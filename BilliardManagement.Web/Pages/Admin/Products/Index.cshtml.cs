using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Products
{
    public class IndexModel : AdminPageModel
    {
        private readonly ProductService _productService;
        public IndexModel(ProductService productService) { _productService = productService; }

        public List<ProductDto> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _productService.GetAllProductsAsync();
                Products = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
