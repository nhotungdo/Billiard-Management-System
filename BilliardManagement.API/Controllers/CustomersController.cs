using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Exceptions;

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
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto dto)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerAsync(id, dto);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result) return NotFound(new { message = "Customer not found." });
            return NoContent();
        }

        [HttpGet("top-spending")]
        public async Task<IActionResult> GetTopSpendingCustomers([FromQuery] int top = 10)
        {
            var result = await _customerService.GetTopSpendersAsync(top);
            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var result = await _customerService.GetCustomerDashboardAsync();
            return Ok(result);
        }
    }
}
