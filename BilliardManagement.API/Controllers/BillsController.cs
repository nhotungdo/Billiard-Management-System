using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Responses;
using BilliardManagement.Business.DTOs;
using System.Security.Claims;

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
            return Ok(ApiResponse<BillDto>.Ok(bill, "Hóa đơn đã được tạo thành công"));
        }

        // thanh toán hóa đơn
        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayBill(Guid id)
        {
            var bill = await _billingService.PayBillAsync(id);
            return Ok(ApiResponse<BillDto>.Ok(bill, "Hóa đơn đã được thanh toán thành công"));
        }

        // lấy tất cả hóa đơn hoặc phân trang/tìm kiếm
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InvoiceQueryParameters query)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
            }

            if (User.IsInRole("Staff"))
            {
                query.StaffId = userId.Value;
            }

            var pagedResult = await _billingService.GetPagedBillsAsync(query);
            return Ok(ApiResponse<PagedResult<BillDto>>.Ok(pagedResult));
        }

        // lấy hóa đơn theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            if (bill == null)
            {
                return NotFound(ApiResponse<object>.Fail("Hóa đơn không tồn tại"));
            }

            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
            }

            if (User.IsInRole("Staff"))
            {
                if (bill.StaffId != userId.Value)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Không có quyền truy cập hóa đơn này"));
                }
            }

            return Ok(ApiResponse<BillDto>.Ok(bill));
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
