using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Responses;

namespace BilliardManagement.Business.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<PagedResult<CustomerDto>> GetPagedCustomersAsync(CustomerQueryParameters query);
        Task<CustomerDto> GetCustomerByIdAsync(Guid id);
        Task<CustomerDto?> GetCustomerByPhoneAsync(string phoneNumber);
        Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto);
        Task<CustomerDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<IEnumerable<CustomerTopSpenderDto>> GetTopSpendersAsync(int top = 10);
        Task<CustomerDashboardDto> GetCustomerDashboardAsync();
        Task<CustomerDto> FindOrCreateCustomerAsync(string fullName, string phoneNumber);
    }
}
