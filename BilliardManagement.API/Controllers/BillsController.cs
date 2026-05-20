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

        public BillsController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        //  tạo hóa đơn mới cho một phiên chơi cụ thể
        [HttpPost("generate/{sessionId}")]
        public async Task<IActionResult> GenerateBill(Guid sessionId, [FromBody] GenerateBillDto dto)
        {
            var bill = await _billingService.GenerateBillAsync(sessionId, dto);
            return Ok(ApiResponse<BillDto>.Ok(bill, "Bill generated successfully"));
        }

        // thanh toán hóa đơn
        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayBill(Guid id)
        {
            var bill = await _billingService.PayBillAsync(id);
            return Ok(ApiResponse<BillDto>.Ok(bill, "Bill paid successfully"));
        }

        // lấy tất cả hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _billingService.GetAllBillsAsync();
            return Ok(ApiResponse<IEnumerable<BillDto>>.Ok(bills));
        }

        // lấy hóa đơn theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            return Ok(ApiResponse<BillDto>.Ok(bill));
        }
    }
}
