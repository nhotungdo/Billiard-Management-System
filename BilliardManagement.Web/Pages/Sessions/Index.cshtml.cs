using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BilliardManagement.Web.Services;
using BilliardManagement.Web.Models;

namespace BilliardManagement.Web.Pages.Sessions
{
    public class IndexModel : AdminOrStaffPageModel
    {
        private readonly TableService _tableService;
        private readonly SessionService _sessionService;
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(TableService tableService, SessionService sessionService,
            InvoiceService invoiceService, ILogger<IndexModel> logger)
        {
            _tableService = tableService;
            _sessionService = sessionService;
            _invoiceService = invoiceService;
            _logger = logger;
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
                _logger.LogError(ex, "LoadDataAsync failed: {Message}", ex.Message);
                ErrorMessage = $"Không thể tải dữ liệu bàn/session: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostStartAsync(Guid tableId)
        {
            _logger.LogInformation("OnPostStart called: tableId={TableId}", tableId);
            try
            {
                var session = await _sessionService.StartSessionAsync(tableId, 1);
                if (session != null)
                {
                    _logger.LogInformation("Session started successfully: sessionId={SessionId}", session.Id);
                    TempData["SuccessMessage"] = "Bắt đầu phiên chơi thành công!";
                }
                else
                {
                    _logger.LogWarning("StartSession returned null for tableId={TableId}", tableId);
                    TempData["ErrorMessage"] = "Không thể bắt đầu phiên chơi.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostStart failed for tableId={TableId}: {Message}", tableId, ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi bắt đầu phiên chơi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEndAsync(Guid sessionId)
        {
            _logger.LogInformation("OnPostEnd called: sessionId={SessionId}", sessionId);
            try
            {
                // Step 1: End the session
                var session = await _sessionService.EndSessionAsync(sessionId);
                if (session == null)
                {
                    _logger.LogWarning("EndSession returned null for sessionId={SessionId}", sessionId);
                    TempData["ErrorMessage"] = "Không thể kết thúc phiên chơi.";
                    return RedirectToPage();
                }

                _logger.LogInformation("Session ended successfully: sessionId={SessionId}, totalPrice={TotalPrice}",
                    session.Id, session.TotalPrice);

                // Step 2: Auto-generate invoice with 0 discount and Cash payment
                try
                {
                    var createInvoiceDto = new CreateInvoiceDto
                    {
                        Discount = 0,
                        PaymentMethod = 0 // Cash
                    };
                    var invoice = await _invoiceService.GenerateInvoiceAsync(sessionId, createInvoiceDto);
                    if (invoice != null)
                    {
                        _logger.LogInformation("Invoice generated successfully: invoiceId={InvoiceId}", invoice.Id);
                        TempData["SuccessMessage"] = $"✅ Kết thúc bàn thành công! Hóa đơn #{invoice.Id.ToString()[..8].ToUpper()} đã được tạo.";
                        return RedirectToPage("/Invoices/Index");
                    }
                    else
                    {
                        _logger.LogWarning("GenerateInvoice returned null for sessionId={SessionId}", sessionId);
                        TempData["SuccessMessage"] = "Kết thúc phiên chơi thành công, nhưng không thể tạo hóa đơn tự động.";
                    }
                }
                catch (Exception invoiceEx)
                {
                    _logger.LogError(invoiceEx, "GenerateInvoice failed for sessionId={SessionId}: {Message}",
                        sessionId, invoiceEx.Message);
                    // Session đã ended thành công, chỉ báo lỗi tạo invoice
                    TempData["SuccessMessage"] = "Kết thúc phiên chơi thành công.";
                    TempData["ErrorMessage"] = $"Lưu ý: Không thể tạo hóa đơn tự động ({invoiceEx.Message}). Vui lòng tạo thủ công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnPostEnd failed for sessionId={SessionId}: {Message}", sessionId, ex.Message);
                TempData["ErrorMessage"] = $"❌ Không thể kết thúc bàn: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
