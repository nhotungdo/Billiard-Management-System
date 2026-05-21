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
    }
}
