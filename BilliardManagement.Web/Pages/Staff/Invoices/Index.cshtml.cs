using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilliardManagement.Web.Pages.Staff.Invoices
{
    public class IndexModel : StaffPageModel
    {
        private readonly InvoiceService _invoiceService;
        public IndexModel(InvoiceService invoiceService) { _invoiceService = invoiceService; }

        public List<InvoiceDto> Invoices { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _invoiceService.GetAllInvoicesAsync();
                Invoices = result?.ToList() ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            return Page();
        }
    }
}
