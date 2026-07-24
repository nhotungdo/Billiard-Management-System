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
    public class CreateProductRequest
    {
        public IFormFile? Image { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int Stock { get; set; } = 0;
    }

    public class UpdateProductRequest
    {
        public IFormFile? Image { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int Stock { get; set; } = 0;
        public string? ExistingImageUrl { get; set; }
    }

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
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters query)
        {
            var pagedResult = await _productService.GetPagedProductsAsync(query);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(pagedResult));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<object>.Fail("Sản phẩm không tồn tại"));

                return Ok(ApiResponse<ProductDto>.Ok(product));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("check-name")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckName([FromQuery] string name, [FromQuery] Guid? excludeId = null)
        {
            var isDuplicate = await _productService.CheckDuplicateNameAsync(name, excludeId);
            return Ok(new { isDuplicate });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
        {
            try
            {
                string? imageUrl = null;
                if (request.Image != null && request.Image.Length > 0)
                {
                    if (!ProductService.IsValidImageExtension(request.Image.FileName))
                        return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận ảnh jpg, jpeg, png"));

                    var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "products");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Image.CopyToAsync(stream);
                    }

                    imageUrl = $"/uploads/products/{fileName}";
                }

                var dto = new CreateProductDto
                {
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    Price = request.Price,
                    Description = request.Description,
                    IsAvailable = request.IsAvailable,
                    Stock = request.Stock,
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
                    CategoryId = product.CategoryId,
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
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductRequest request)
        {
            try
            {
                string? imageUrl = request.ExistingImageUrl;
                if (request.Image != null && request.Image.Length > 0)
                {
                    if (!ProductService.IsValidImageExtension(request.Image.FileName))
                        return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận ảnh jpg, jpeg, png"));

                    var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "products");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Image.CopyToAsync(stream);
                    }

                    imageUrl = $"/uploads/products/{fileName}";
                }

                var dto = new CreateProductDto
                {
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    Price = request.Price,
                    Description = request.Description,
                    IsAvailable = request.IsAvailable,
                    Stock = request.Stock,
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

                return Ok(ApiResponse<object?>.Ok(null, "Xóa sản phẩm thành công"));
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
