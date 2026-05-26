using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using System.Linq;

using BilliardManagement.Common.Responses;

namespace BilliardManagement.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid userId)
        {
            var sessionId = dto.SessionId != Guid.Empty ? dto.SessionId : dto.TableSessionId;
            if (sessionId == Guid.Empty)
                throw new CustomException("tableSessionId is required", 400);

            var session = await _unitOfWork.Repository<TableSession>().GetByIdAsync(sessionId);
            if (session == null)
                throw new CustomException("Session not found", 404);
            if (session.IsFinished || session.Status != SessionStatus.Active)
                throw new CustomException("Cannot order: session is not active", 400);

            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
            if (table == null || table.Status != TableStatus.Playing)
                throw new CustomException("Cannot order: table is not currently playing", 400);

            var order = new Order
            {
                TableSessionId = sessionId,
                OrderedBy = userId,
                Status = OrderStatus.Pending,
                TotalAmount = 0
            };

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Quantity <= 0) throw new CustomException("Quantity must be greater than 0", 400);

                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(itemDto.ProductId);
                if (product == null) throw new CustomException($"Product {itemDto.ProductId} not found", 404);
                if (product.StockQuantity < itemDto.Quantity) throw new CustomException($"Not enough stock for {product.ProductName}", 400);

                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Repository<Product>().Update(product);

                var lineTotal = itemDto.Quantity * product.Price;
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = lineTotal
                };

                order.TotalAmount += lineTotal;
                order.OrderItems.Add(orderItem);
            }

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order == null) throw new CustomException("Order not found", 404);
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(
                o => o.Id == id, "OrderItems");
            if (order == null) throw new CustomException("Không tìm thấy đơn hàng", 404);

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                throw new CustomException("Không thể thay đổi trạng thái của đơn hàng đã hoàn thành hoặc đã hủy", 400);
            }

            // Restore stock if transitioning to Cancelled
            if (status == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _unitOfWork.Repository<Product>().Update(product);
                    }
                }
            }

            order.Status = status;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<PagedResult<OrderDto>> GetPagedOrdersAsync(OrderQueryParameters query)
        {
            var filters = new List<System.Linq.Expressions.Expression<System.Func<Order, bool>>>();

            if (query.Status.HasValue)
            {
                var statusEnum = (OrderStatus)query.Status.Value;
                filters.Add(o => o.Status == statusEnum);
            }
            if (query.SessionId.HasValue)
            {
                filters.Add(o => o.TableSessionId == query.SessionId.Value);
            }

            Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(o => o.TotalAmount) : q.OrderBy(o => o.TotalAmount);
                }
            }
            else
            {
                orderBy = q => q.OrderByDescending(o => o.Id);
            }

            var (items, totalCount) = await _unitOfWork.Repository<Order>().GetPagedAsync(
                filters: filters,
                orderBy: orderBy,
                includeProperties: null,
                page: query.PageNumber,
                pageSize: query.PageSize
            );

            var mappedItems = _mapper.Map<IEnumerable<OrderDto>>(items);
            return new PagedResult<OrderDto>(mappedItems, query.PageNumber, query.PageSize, totalCount);
        }
    }
}
