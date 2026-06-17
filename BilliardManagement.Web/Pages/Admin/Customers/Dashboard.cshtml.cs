using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Customers
{
    public class DashboardModel : AdminPageModel
    {
        private readonly CustomerService _customerService;

        public DashboardModel(CustomerService customerService)
        {
            _customerService = customerService;
        }

        public CustomerDashboardDto? DashboardData { get; set; }
        public List<CustomerTopSpenderDto>? TopSpenders { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                DashboardData = await _customerService.GetCustomerDashboardAsync();
                TopSpenders = await _customerService.GetTopSpendingCustomersAsync(10);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return Page();
        }
    }
}
