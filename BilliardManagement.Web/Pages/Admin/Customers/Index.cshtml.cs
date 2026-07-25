using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Admin.Customers
{
    public class IndexModel : AdminPageModel
    {
        private readonly CustomerService _customerService;

        public IndexModel(CustomerService customerService)
        {
            _customerService = customerService;
        }

        public PagedResult<CustomerDto>? Customers { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IsDescending { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1 || PageSize > 100) PageSize = 10;
            SearchTerm = SearchTerm?.Trim();

            var allowedSorts = new[] { "TotalSpent", "TotalVisits", "LastVisitDate", "CustomerName" };
            if (!string.IsNullOrEmpty(SortBy) && !allowedSorts.Contains(SortBy))
            {
                SortBy = "TotalSpent";
            }

            try
            {
                Customers = await _customerService.GetPagedCustomersAsync(PageNumber, PageSize, SearchTerm, SortBy, IsDescending);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return Page();
        }

        [BindProperty]
        public CustomerCreateDto NewCustomer { get; set; } = new();

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                var result = await _customerService.CreateCustomerAsync(NewCustomer);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm khách hàng: {ex.Message}";
            }
            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, IsDescending });
        }

        [BindProperty]
        public CustomerUpdateDto EditCustomer { get; set; } = new();

        public async Task<IActionResult> OnPostEditAsync(Guid id)
        {
            try
            {
                var result = await _customerService.UpdateCustomerAsync(id, EditCustomer);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật khách hàng: {ex.Message}";
            }
            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, IsDescending });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _customerService.DeleteCustomerAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa khách hàng thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng này.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa khách hàng: {ex.Message}";
            }
            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, IsDescending });
        }
    }
}
