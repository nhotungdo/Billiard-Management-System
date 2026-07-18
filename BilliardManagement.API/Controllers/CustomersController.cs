using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Common.Responses;
using Microsoft.AspNetCore.Authorization;

namespace BilliardManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedCustomers([FromQuery] CustomerQueryParameters query)
        {
            var result = await _customerService.GetPagedCustomersAsync(query);
            return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(ApiResponse<CustomerDto>.Ok(customer));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(dto);
                return Ok(ApiResponse<CustomerDto>.Ok(customer));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto dto)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerAsync(id, dto);
                return Ok(ApiResponse<CustomerDto>.Ok(customer));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result) return NotFound(ApiResponse<object>.Fail("Customer not found."));
            return Ok(ApiResponse<object>.Ok(null));
        }

        [HttpGet("top-spending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopSpendingCustomers([FromQuery] int top = 10)
        {
            var result = await _customerService.GetTopSpendersAsync(top);
            return Ok(ApiResponse<IEnumerable<CustomerTopSpenderDto>>.Ok(result));
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var result = await _customerService.GetCustomerDashboardAsync();
            return Ok(ApiResponse<CustomerDashboardDto>.Ok(result));
        }
    }
}
