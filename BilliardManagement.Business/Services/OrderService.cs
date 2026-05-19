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
            var session = await _unitOfWork.Repository<TableSession>().GetByIdAsync(dto.SessionId);
            if (session == null || session.Status != SessionStatus.Active) 
                throw new CustomException("Active session not found", 400);

            var order = new Order
            {
                TableSessionId = dto.SessionId,
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

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };
                
                order.TotalAmount += orderItem.Quantity * orderItem.UnitPrice;
                order.OrderItems.Add(orderItem);
            }

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderDto>(order);
        }
    }
}
