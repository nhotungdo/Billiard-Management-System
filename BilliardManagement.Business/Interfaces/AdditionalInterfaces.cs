using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;

using BilliardManagement.Common.Responses;

namespace BilliardManagement.Business.Interfaces
{
    public interface IBillingService
    {
        Task<BillDto> GenerateBillAsync(Guid sessionId, GenerateBillDto dto);
        Task<BillDto> GetBillByIdAsync(Guid id);
        Task<IEnumerable<BillDto>> GetAllBillsAsync();
        Task<PagedResult<BillDto>> GetPagedBillsAsync(InvoiceQueryParameters query);
        Task<BillDto> PayBillAsync(Guid id);
    }

    public interface IReportService
    {
        Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(int days);
        Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupType);
        Task<object> GetDashboardAnalyticsAsync();
    }

    public interface IShiftService
    {
        Task<ShiftDto> CheckInAsync(Guid userId);
        Task<ShiftDto> CheckOutAsync(Guid shiftId);
        Task<IEnumerable<ShiftDto>> GetTodayShiftsAsync();
    }

    public interface IRevenueService
    {
        Task<PersonalRevenueDto> GetPersonalRevenueAsync(Guid userId, DateTime? fromDate, DateTime? toDate);
        Task<RevenueSummaryDto> GetTotalRevenueAsync(RevenueFilterQuery filter);
        Task<PersonalRevenueDto> GetRevenueByStaffAsync(Guid staffId, DateTime? fromDate, DateTime? toDate);
    }
}
