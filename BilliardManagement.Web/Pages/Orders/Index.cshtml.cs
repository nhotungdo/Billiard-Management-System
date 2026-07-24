using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Orders
{
    public class IndexModel : AdminOrStaffPageModel
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly SessionService _sessionService;

        public IndexModel(OrderService orderService, ProductService productService, SessionService sessionService)
        {
            _orderService = orderService;
            _productService = productService;
            _sessionService = sessionService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? SessionId { get; set; }

        public List<OrderDto> Orders { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public List<SessionDto> ActiveSessions { get; set; } = new();
        
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public List<CreateOrderItemViewModel> OrderItems { get; set; } = new();

        public class CreateOrderItemViewModel
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                if (SessionId.HasValue && SessionId.Value != Guid.Empty)
                {
                    var prodList = await _productService.GetAllProductsAsync();
                    if (prodList != null)
                    {
                        Products = prodList.Where(p => p.Stock > 0).ToList();
                    }
                }
                else
                {
                    var ordList = await _orderService.GetAllOrdersAsync();
                    if (ordList != null)
                    {
                        Orders = ordList;
                    }
                    
                    var sessList = await _sessionService.GetActiveSessionsAsync();
                    if (sessList != null)
                    {
                        ActiveSessions = sessList;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load data: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostCreateOrderAsync()
        {
            if (!SessionId.HasValue || SessionId.Value == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn bàn chơi trước khi đặt món.";
                return RedirectToPage("/Sessions/Index");
            }

            // Filter items with quantity > 0
            var items = OrderItems
                .Where(i => i.Quantity > 0)
                .Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                })
                .ToList();

            if (items.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn số lượng lớn hơn 0 cho ít nhất một món.";
                return RedirectToPage(new { sessionId = SessionId });
            }

            try
            {
                var createOrderDto = new CreateOrderDto
                {
                    SessionId = SessionId.Value,
                    Items = items
                };

                var order = await _orderService.CreateOrderAsync(createOrderDto);
                if (order != null)
                {
                    TempData["SuccessMessage"] = "Đặt món thành công!";
                    return RedirectToPage("/Sessions/Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Đặt món thất bại. Vui lòng thử lại.";
                    return RedirectToPage(new { sessionId = SessionId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error placing order: {ex.Message}";
                return RedirectToPage(new { sessionId = SessionId });
            }
        }
    }
}
