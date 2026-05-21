using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.Services;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using Microsoft.AspNetCore.SignalR;
using BilliardManagement.API.Hubs;
using System.Security.Claims;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IHubContext<ProductHub> _productHub;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            IHubContext<ProductHub> productHub,
            IWebHostEnvironment env,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _productHub = productHub;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            [FromForm] IFormFile? image,
            [FromForm] string name,
            [FromForm] string category,
            [FromForm] decimal price,
            [FromForm] string? description,
            [FromForm] bool isAvailable = true,
            [FromForm] int stock = 0)
        {
            try
            {
                string? imageUrl = null;
                if (image != null && image.Length > 0)
                {
                    if (!ProductService.IsValidImageExtension(image.FileName))
                        return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận ảnh jpg, jpeg, png"));

                    var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "products");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    imageUrl = $"/uploads/products/{fileName}";
                }

                var dto = new CreateProductDto
                {
                    Name = name,
                    Category = category,
                    Price = price,
                    Description = description,
                    IsAvailable = isAvailable,
                    Stock = stock,
                    ImageUrl = imageUrl
                };

                var userId = GetUserId();
                var product = await _productService.CreateProductAsync(dto, userId);

                _logger.LogInformation(
                    "User {UserId} created product {ProductId} ({ProductName}) at {Time}",
                    userId, product.Id, product.Name, DateTime.UtcNow);

                var payload = new ProductCreatedDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Category = product.Category,
                    ImageUrl = product.ImageUrl,
                    IsAvailable = product.IsAvailable
                };

                await _productHub.Clients.All.SendAsync("ProductCreated", payload);
                await _productHub.Clients.All.SendAsync("ReceiveProductUpdate", "Product created");

                return Ok(ApiResponse<ProductDto>.Ok(product, "Tạo thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Create product failed");
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromForm] IFormFile? image,
            [FromForm] string name,
            [FromForm] string category,
            [FromForm] decimal price,
            [FromForm] string? description,
            [FromForm] bool isAvailable = true,
            [FromForm] int stock = 0,
            [FromForm] string? existingImageUrl = null)
        {
            try
            {
                string? imageUrl = existingImageUrl;
                if (image != null && image.Length > 0)
                {
                    if (!ProductService.IsValidImageExtension(image.FileName))
                        return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận ảnh jpg, jpeg, png"));

                    var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "products");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    imageUrl = $"/uploads/products/{fileName}";
                }

                var dto = new CreateProductDto
                {
                    Name = name,
                    Category = category,
                    Price = price,
                    Description = description,
                    IsAvailable = isAvailable,
                    Stock = stock,
                    ImageUrl = imageUrl
                };

                var userId = GetUserId();
                var product = await _productService.UpdateProductAsync(id, dto, userId);

                _logger.LogInformation(
                    "User {UserId} updated product {ProductId} ({ProductName}) at {Time}",
                    userId, product.Id, product.Name, DateTime.UtcNow);

                await _productHub.Clients.All.SendAsync("ReceiveProductUpdate", "Product updated");

                return Ok(ApiResponse<ProductDto>.Ok(product, "Cập nhật thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update product failed for {ProductId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Fail("Product not found"));

                _logger.LogInformation("Product {ProductId} deleted at {Time}", id, DateTime.UtcNow);
                await _productHub.Clients.All.SendAsync("ReceiveProductUpdate", "Product deleted");

                return Ok(ApiResponse<object>.Ok(null, "Xóa sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Delete product failed for {ProductId}", id);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
