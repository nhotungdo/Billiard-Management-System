using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Responses;
using AutoMapper;

namespace BilliardManagement.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly BilliardManagementDbContext _context;
        private readonly IMapper _mapper;

        public CustomerService(BilliardManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<PagedResult<CustomerDto>> GetPagedCustomersAsync(CustomerQueryParameters query)
        {
            var customersQuery = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                customersQuery = customersQuery.Where(c => c.FullName.Contains(query.SearchTerm) || c.PhoneNumber.Contains(query.SearchTerm));
            }
            if (!string.IsNullOrEmpty(query.PhoneNumber))
            {
                customersQuery = customersQuery.Where(c => c.PhoneNumber.Contains(query.PhoneNumber));
            }
            if (query.FromDate.HasValue)
            {
                customersQuery = customersQuery.Where(c => c.LastVisitDate >= query.FromDate.Value);
            }
            if (query.ToDate.HasValue)
            {
                customersQuery = customersQuery.Where(c => c.LastVisitDate <= query.ToDate.Value);
            }
            if (query.MinTotalSpent.HasValue)
            {
                customersQuery = customersQuery.Where(c => c.TotalSpent >= query.MinTotalSpent.Value);
            }

            var totalCount = await customersQuery.CountAsync();

            customersQuery = query.SortBy?.ToLower() switch
            {
                "fullname" => query.IsDescending ? customersQuery.OrderByDescending(c => c.FullName) : customersQuery.OrderBy(c => c.FullName),
                "totalspent" => query.IsDescending ? customersQuery.OrderByDescending(c => c.TotalSpent) : customersQuery.OrderBy(c => c.TotalSpent),
                "totalvisits" => query.IsDescending ? customersQuery.OrderByDescending(c => c.TotalVisits) : customersQuery.OrderBy(c => c.TotalVisits),
                "lastvisitdate" => query.IsDescending ? customersQuery.OrderByDescending(c => c.LastVisitDate) : customersQuery.OrderBy(c => c.LastVisitDate),
                _ => customersQuery.OrderByDescending(c => c.CreatedAt)
            };

            var customers = await customersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<CustomerDto>
            {
                Items = _mapper.Map<IEnumerable<CustomerDto>>(customers),
                TotalCount = totalCount,
                Page = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phoneNumber)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto)
        {
            if (await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            {
                throw new InvalidOperationException("Phone number already exists.");
            }

            var customer = new Customer
            {
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            if (customer.PhoneNumber != dto.PhoneNumber && await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            {
                throw new InvalidOperationException("Phone number already exists.");
            }

            customer.FullName = dto.FullName;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CustomerTopSpenderDto>> GetTopSpendersAsync(int top = 10)
        {
            var topSpenders = await _context.Customers
                .OrderByDescending(c => c.TotalSpent)
                .Take(top)
                .ToListAsync();

            var result = topSpenders.Select((c, index) => new CustomerTopSpenderDto
            {
                Rank = index + 1,
                Id = c.Id,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                TotalVisits = c.TotalVisits,
                TotalSpent = c.TotalSpent
            }).ToList();

            return result;
        }

        public async Task<CustomerDashboardDto> GetCustomerDashboardAsync()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newCustomers = await _context.Customers.CountAsync(c => c.CreatedAt >= thirtyDaysAgo);
            
            var retentionRate = totalCustomers > 0 
                ? (decimal)(await _context.Customers.CountAsync(c => c.TotalVisits > 1)) / totalCustomers * 100 
                : 0;

            return new CustomerDashboardDto
            {
                TotalCustomers = totalCustomers,
                NewCustomersLast30Days = newCustomers,
                RetentionRate = Math.Round(retentionRate, 2)
            };
        }

        public async Task<CustomerDto> FindOrCreateCustomerAsync(string fullName, string phoneNumber)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            return _mapper.Map<CustomerDto>(customer);
        }
    }
}
