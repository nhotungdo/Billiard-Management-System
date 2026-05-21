using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Orders
{
    public class IndexModel : StaffPageModel
    {
        private readonly OrderService _orderService;
        public IndexModel(OrderService orderService) { _orderService = orderService; }

        public List<OrderDto> Orders { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _orderService.GetAllOrdersAsync();
                Orders = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
