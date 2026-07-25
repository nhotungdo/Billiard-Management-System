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
            var session = await _unitOfWork.Repository<TableSession>().GetFirstOrDefaultAsync(
                s => s.Id == sessionId, "Orders,Orders.OrderItems,SessionCombos,BilliardTable,User");
            if (session == null) throw new CustomException("Session not found", 404);

            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                o => o.TableSessionId == sessionId && !o.IsComboOrder && o.Status != OrderStatus.Cancelled);

            decimal foodPrice = orders.Sum(o => o.TotalAmount);
            decimal comboPrice = session.SessionCombos != null && session.SessionCombos.Any() 
                ? session.SessionCombos.Sum(sc => sc.Price) 
                : session.ComboPrice;
            decimal tableFeeAfterCombo = session.TotalPrice;
            decimal subtotal = comboPrice + foodPrice + tableFeeAfterCombo;

            var existingInvoice = await _unitOfWork.Repository<Invoice>().GetFirstOrDefaultAsync(i => i.TableSessionId == sessionId);
            Invoice invoice;
            if (existingInvoice != null)
            {
                existingInvoice.Subtotal = subtotal;
                existingInvoice.TotalAmount = subtotal;
                existingInvoice.PaymentMethod = dto.PaymentMethod;
                existingInvoice.IsPaid = true;
                _unitOfWork.Repository<Invoice>().Update(existingInvoice);
                await _unitOfWork.SaveChangesAsync();
                invoice = existingInvoice;
            }
            else
            {
                invoice = new Invoice
                {
                    TableSessionId = sessionId,
                    CustomerId = session.CustomerId,
                    Subtotal = subtotal,
                    TotalAmount = subtotal,
                    PaymentMethod = dto.PaymentMethod,
                    IsPaid = true
                };

                await _unitOfWork.Repository<Invoice>().AddAsync(invoice);

                if (session.CustomerId.HasValue)
                {
                    var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(session.CustomerId.Value);
                    if (customer != null)
                    {
                        customer.TotalVisits += 1;
                        customer.TotalPlayHours += (decimal)session.DurationHours;
                        customer.TotalSpent += invoice.TotalAmount;
                        customer.LastVisitDate = DateTime.UtcNow;
                        if (!customer.FirstVisitDate.HasValue)
                        {
                            customer.FirstVisitDate = DateTime.UtcNow;
                        }
                        _unitOfWork.Repository<Customer>().Update(customer);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }

            var billDto = _mapper.Map<BillDto>(invoice);
            billDto.ComboFee = comboPrice;
            billDto.ServiceFee = foodPrice;
            billDto.PlayingFee = tableFeeAfterCombo;
            billDto.TableFeeAfterCombo = tableFeeAfterCombo;
            billDto.AppliedCombos = _mapper.Map<List<SessionComboDto>>(session.SessionCombos?.OrderBy(sc => sc.AppliedAt).ToList() ?? new List<SessionCombo>());

            if (session.ComboEndTime.HasValue && session.EndTime.HasValue && session.EndTime.Value > session.ComboEndTime.Value)
            {
                var overSecs = (session.EndTime.Value - session.ComboEndTime.Value).TotalSeconds;
                billDto.OverComboMinutes = (int)Math.Ceiling(overSecs / 60.0);
            }

            return billDto;
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
            if (query.StaffId.HasValue)
            {
                filters.Add(i => i.TableSession != null && i.TableSession.UserId == query.StaffId.Value);
            }

            Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(i => i.TotalAmount) : q.OrderBy(i => i.TotalAmount);
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
