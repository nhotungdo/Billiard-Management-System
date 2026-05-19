using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;
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

            decimal ordersTotal = session.Orders?.Sum(o => o.TotalAmount) ?? 0;
            decimal sessionTotal = session.TotalPrice ?? 0;
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
            var bills = await _unitOfWork.Repository<Invoice>().GetAllAsync();
            return _mapper.Map<IEnumerable<BillDto>>(bills);
        }

        public async Task<BillDto> GetBillByIdAsync(Guid id)
        {
            var bill = await _unitOfWork.Repository<Invoice>().GetByIdAsync(id);
            if (bill == null) throw new CustomException("Bill not found", 404);
            return _mapper.Map<BillDto>(bill);
        }

        public async Task<BillDto> PayBillAsync(Guid id)
        {
            var bill = await _unitOfWork.Repository<Invoice>().GetByIdAsync(id);
            if (bill == null) throw new CustomException("Bill not found", 404);

            bill.IsPaid = true;
            _unitOfWork.Repository<Invoice>().Update(bill);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BillDto>(bill);
        }
    }
}
