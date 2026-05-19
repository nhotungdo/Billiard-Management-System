using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using System.Linq;

namespace BilliardManagement.Business.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShiftService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShiftDto> CheckInAsync(Guid userId)
        {
            var activeShift = await _unitOfWork.Repository<Shift>().GetFirstOrDefaultAsync(s => s.UserId == userId && s.EndTime == null);
            if (activeShift != null) throw new CustomException("User already checked in", 400);

            var shift = new Shift
            {
                UserId = userId,
                StartTime = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Shift>().AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task<ShiftDto> CheckOutAsync(Guid shiftId)
        {
            var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(shiftId);
            if (shift == null || shift.EndTime != null) throw new CustomException("Invalid or already ended shift", 400);

            shift.EndTime = DateTime.UtcNow;

            // Calculate revenue collected by this staff during their shift
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i => i.CreatedAt >= shift.StartTime && i.CreatedAt <= shift.EndTime && i.IsPaid);
            shift.TotalRevenue = invoices.Sum(i => i.TotalAmount);

            _unitOfWork.Repository<Shift>().Update(shift);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task<IEnumerable<ShiftDto>> GetTodayShiftsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var shifts = await _unitOfWork.Repository<Shift>().GetAllAsync(s => s.StartTime >= today);
            return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
        }
    }
}
