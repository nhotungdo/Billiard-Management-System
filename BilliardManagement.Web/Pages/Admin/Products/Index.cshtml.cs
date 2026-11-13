using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Products
{
    public class IndexModel : AdminPageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public List<ProductDto> Products { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public int AvailableCount => Products.Count(p => p.IsAvailable);
        public int OutOfStockCount => Products.Count(p => p.Stock <= 0);
        public decimal TotalStockValue => Products.Sum(p => p.Stock * p.Price);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _productService.GetAllProductsAsync();
                Products = result?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            try
            {
                Categories = await _productService.GetAllCategoriesAsync() ?? new();
            }
            catch
            {
                Categories = new();
            }

            return Page();
        }
    }
}
