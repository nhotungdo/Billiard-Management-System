using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Sessions
{
    public class IndexModel : PageModel
    {
        private readonly TableService _tableService;
        private readonly SessionService _sessionService;
        private readonly InvoiceService _invoiceService;

        public IndexModel(TableService tableService, SessionService sessionService, InvoiceService invoiceService)
        {
            _tableService = tableService;
            _sessionService = sessionService;
            _invoiceService = invoiceService;
        }

        public List<TableDto> Tables { get; set; } = new();
        public List<SessionDto> ActiveSessions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tablesList = await _tableService.GetAllTablesAsync();
                if (tablesList != null)
                {
                    Tables = tablesList;
                }

                var sessionsList = await _sessionService.GetActiveSessionsAsync();
                if (sessionsList != null)
                {
                    ActiveSessions = sessionsList;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load session/table data: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostStartAsync(Guid tableId)
        {
            try
            {
                var session = await _sessionService.StartSessionAsync(tableId);
                if (session != null)
                {
                    TempData["SuccessMessage"] = "Session started successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to start session.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error starting session: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEndAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionService.EndSessionAsync(sessionId);
                if (session != null)
                {
                    // Auto-generate invoice with 0 discount and Cash (0)
                    var createInvoiceDto = new CreateInvoiceDto
                    {
                        Discount = 0,
                        PaymentMethod = 0 // Cash
                    };
                    var invoice = await _invoiceService.GenerateInvoiceAsync(sessionId, createInvoiceDto);
                    if (invoice != null)
                    {
                        TempData["SuccessMessage"] = $"Session ended and Invoice #{invoice.Id.ToString().Substring(0, 8)} generated successfully!";
                        return RedirectToPage("/Invoices/Index");
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Session ended successfully, but invoice generation failed.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to end session.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error ending session: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
