using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Business.Interfaces
{
    public interface IBillingService
    {
        Task<BillDto> GenerateBillAsync(Guid sessionId, GenerateBillDto dto);
        Task<BillDto> GetBillByIdAsync(Guid id);
        Task<IEnumerable<BillDto>> GetAllBillsAsync();
        Task<BillDto> PayBillAsync(Guid id);
    }

    public interface IReportService
    {
        Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(int days);
        Task<object> GetDashboardAnalyticsAsync();
    }

    public interface IShiftService
    {
        Task<ShiftDto> CheckInAsync(Guid userId);
        Task<ShiftDto> CheckOutAsync(Guid shiftId);
        Task<IEnumerable<ShiftDto>> GetTodayShiftsAsync();
    }
}
