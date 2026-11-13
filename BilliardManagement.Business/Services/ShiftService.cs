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

        // Constructor
        public ShiftService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // khi nhân viên check-in, tạo một ca mới với thời gian bắt đầu và lưu vào cơ sở dữ liệu
        public async Task<ShiftDto> CheckInAsync(Guid userId)
        {
            var activeShift = await _unitOfWork.Repository<Shift>().GetFirstOrDefaultAsync(s => s.UserId == userId && s.EndTime == null);
            if (activeShift != null) throw new CustomException("User already checked in", 400);

            var shift = new Shift
            {
                UserId = userId,
                StartTime = DateTime.UtcNow,
                ShiftDate = DateTime.UtcNow.Date
            };

            await _unitOfWork.Repository<Shift>().AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShiftDto>(shift);
        }

        // khi nhân viên check-out, cập nhật thời gian kết thúc của ca làm việc và tính toán doanh thu nếu cần thiết
        public async Task<ShiftDto> CheckOutAsync(Guid shiftId, Guid callerId, bool canManageStore)
        {
            var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(shiftId);
            if (shift == null) throw new CustomException("Không tìm thấy ca làm việc", 404);
            if (!canManageStore && shift.UserId != callerId)
                throw new CustomException("Bạn chỉ được kết thúc ca làm việc của mình", 403);
            if (shift.EndTime != null) throw new CustomException("Ca làm việc đã kết thúc", 400);

            shift.EndTime = DateTime.UtcNow;

            // Match personal revenue attribution: the session owner, within this store and shift.
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i =>
                i.TableSession != null && i.TableSession.StoreId == shift.StoreId &&
                i.TableSession.UserId == shift.UserId && i.CreatedAt >= shift.StartTime && i.CreatedAt <= shift.EndTime);
            shift.TotalRevenue = invoices.Sum(i => i.TotalAmount);

            _unitOfWork.Repository<Shift>().Update(shift);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShiftDto>(shift);
        }

        // lấy danh sách các ca làm việc của ngày hôm nay, bao gồm cả ca đang hoạt động và đã kết thúc
        public async Task<IEnumerable<ShiftDto>> GetTodayShiftsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var shifts = await _unitOfWork.Repository<Shift>().GetAllAsync(s => s.StartTime >= today);
            return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
        }
    }
}
