using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;
using BilliardManagement.Models.Enums;
using BilliardManagement.Common.Responses;
using BilliardManagement.Common.Exceptions;

namespace BilliardManagement.Business.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BillingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BillDto> GenerateBillAsync(Guid sessionId, GenerateBillDto dto)
        {
            var session = await _unitOfWork.Repository<TableSession>().GetFirstOrDefaultAsync(s => s.Id == sessionId, "Orders,Orders.OrderItems");
            if (session == null) throw new CustomException("Session not found", 404);

            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(o => o.TableSessionId == sessionId && o.Status == OrderStatus.Completed);
            decimal ordersTotal = orders.Sum(o => o.TotalAmount);
            decimal sessionTotal = session.TotalPrice;
            decimal subtotal = ordersTotal + sessionTotal;

            var invoice = new Invoice
            {
                TableSessionId = sessionId,
                Subtotal = subtotal,
                Discount = dto.Discount,
                TotalAmount = subtotal - dto.Discount,
                PaymentMethod = dto.PaymentMethod,
                IsPaid = false
            };

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BillDto>(invoice);
        }

        public async Task<IEnumerable<BillDto>> GetAllBillsAsync()
        {
            var bills = await _unitOfWork.Repository<Invoice>().GetAllAsync(includeProperties: "TableSession,TableSession.BilliardTable,TableSession.User");
            return _mapper.Map<IEnumerable<BillDto>>(bills);
        }

        public async Task<BillDto> GetBillByIdAsync(Guid id)
        {
            var bill = await _unitOfWork.Repository<Invoice>().GetFirstOrDefaultAsync(i => i.Id == id, "TableSession,TableSession.BilliardTable,TableSession.User");
            if (bill == null) throw new CustomException("Bill not found", 404);
            return _mapper.Map<BillDto>(bill);
        }

        public async Task<BillDto> PayBillAsync(Guid id)
        {
            var bill = await _unitOfWork.Repository<Invoice>().GetFirstOrDefaultAsync(i => i.Id == id, "TableSession,TableSession.BilliardTable,TableSession.User");
            if (bill == null) throw new CustomException("Bill not found", 404);

            bill.IsPaid = true;
            _unitOfWork.Repository<Invoice>().Update(bill);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BillDto>(bill);
        }

        public async Task<PagedResult<BillDto>> GetPagedBillsAsync(InvoiceQueryParameters query)
        {
            var filters = new List<System.Linq.Expressions.Expression<System.Func<Invoice, bool>>>();

            if (query.IsPaid.HasValue)
            {
                if (query.IsPaid.Value == false)
                {
                    filters.Add(i => false);
                }
            }
            if (query.PaymentMethod.HasValue)
            {
                var method = (PaymentMethod)query.PaymentMethod.Value;
                filters.Add(i => i.PaymentMethod == method);
            }

            Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(i => i.TotalAmount) : q.OrderBy(i => i.TotalAmount);
                }
                else if (query.SortBy.Equals("Discount", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(i => i.Discount) : q.OrderBy(i => i.Discount);
                }
                else if (query.SortBy.Equals("Subtotal", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(i => i.Subtotal) : q.OrderBy(i => i.Subtotal);
                }
                else if (query.SortBy.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) || query.SortBy.Equals("Date", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(i => i.CreatedAt) : q.OrderBy(i => i.CreatedAt);
                }
            }
            else
            {
                orderBy = q => q.OrderByDescending(i => i.CreatedAt);
            }

            var (items, totalCount) = await _unitOfWork.Repository<Invoice>().GetPagedAsync(
                filters: filters,
                orderBy: orderBy,
                includeProperties: "TableSession,TableSession.BilliardTable,TableSession.User",
                page: query.PageNumber,
                pageSize: query.PageSize
            );

            var mappedItems = _mapper.Map<IEnumerable<BillDto>>(items);
            return new PagedResult<BillDto>(mappedItems, query.PageNumber, query.PageSize, totalCount);
        }
    }
}
