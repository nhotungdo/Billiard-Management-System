using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BillsController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly ILogger<BillsController> _logger;

        public BillsController(IBillingService billingService, ILogger<BillsController> logger)
        {
            _billingService = billingService;
            _logger = logger;
        }

        // Tạo hóa đơn mới cho một phiên chơi cụ thể
        [HttpPost("generate/{sessionId}")]
        public async Task<IActionResult> GenerateBill(Guid sessionId, [FromBody] GenerateBillDto dto)
        {
            try
            {
                _logger.LogInformation("GenerateBill called: sessionId={SessionId}, discount={Discount}, paymentMethod={PaymentMethod}",
                    sessionId, dto.Discount, dto.PaymentMethod);

                var bill = await _billingService.GenerateBillAsync(sessionId, dto);

                _logger.LogInformation("GenerateBill success: billId={BillId}, total={Total}", bill.Id, bill.Total);
                return Ok(ApiResponse<BillDto>.Ok(bill, "Tạo hóa đơn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateBill failed for sessionId={SessionId}: {Message}", sessionId, ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Thanh toán hóa đơn
        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayBill(Guid id)
        {
            try
            {
                _logger.LogInformation("PayBill called: billId={BillId}", id);
                var bill = await _billingService.PayBillAsync(id);
                _logger.LogInformation("PayBill success: billId={BillId}", bill.Id);
                return Ok(ApiResponse<BillDto>.Ok(bill, "Thanh toán thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayBill failed for billId={BillId}: {Message}", id, ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Lấy tất cả hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bills = await _billingService.GetAllBillsAsync();
                return Ok(ApiResponse<IEnumerable<BillDto>>.Ok(bills));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll bills failed: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        // Lấy hóa đơn theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var bill = await _billingService.GetBillByIdAsync(id);
                return Ok(ApiResponse<BillDto>.Ok(bill));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById bill failed for id={Id}: {Message}", id, ex.Message);
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
