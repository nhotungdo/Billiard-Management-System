using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Models;

namespace BilliardManagement.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> GetDashboardAnalyticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i => i.CreatedAt >= today);
            var activeSessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(s => s.Status == Models.Enums.SessionStatus.Active);
            
            return new
            {
                TodayRevenue = invoices.Sum(i => i.TotalAmount),
                ActiveTables = activeSessions.Count(),
                TotalOrdersToday = invoices.Count()
            };
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(i => i.CreatedAt >= startDate);

            return invoices.GroupBy(i => i.CreatedAt.Date)
                           .Select(g => new DailyRevenueDto
                           {
                               Date = g.Key,
                               TotalRevenue = g.Sum(i => i.TotalAmount)
                           })
                           .OrderBy(x => x.Date);
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupType)
        {
            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

            if (groupType.ToLower() == "month")
            {
                return invoices.GroupBy(i => new DateTime(i.CreatedAt.Year, i.CreatedAt.Month, 1))
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
            else if (groupType.ToLower() == "year")
            {
                return invoices.GroupBy(i => new DateTime(i.CreatedAt.Year, 1, 1))
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
            else // Default to day
            {
                return invoices.GroupBy(i => i.CreatedAt.Date)
                               .Select(g => new DailyRevenueDto
                               {
                                   Date = g.Key,
                                   TotalRevenue = g.Sum(i => i.TotalAmount)
                               })
                               .OrderBy(x => x.Date);
            }
        }
    }
}
