using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Invoices
{
    public class IndexModel : PageModel
    {
        private readonly InvoiceService _invoiceService;

        public IndexModel(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public List<InvoiceDto> Invoices { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var list = await _invoiceService.GetAllInvoicesAsync();
                if (list != null)
                {
                    Invoices = list;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load invoices: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostPayAsync(Guid id)
        {
            try
            {
                var invoice = await _invoiceService.PayInvoiceAsync(id);
                if (invoice != null && invoice.IsPaid)
                {
                    TempData["SuccessMessage"] = "Invoice paid successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to process invoice payment.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error paying invoice: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
