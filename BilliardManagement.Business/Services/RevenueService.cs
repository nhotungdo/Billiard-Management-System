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
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PersonalRevenueDto> GetPersonalRevenueAsync(Guid userId, DateTime? fromDate, DateTime? toDate)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                i => i.TableSession != null && i.TableSession.UserId == userId,
                includeProperties: "TableSession,TableSession.BilliardTable"
            );
            var invoicesList = invoices.ToList();

            var today = DateTime.UtcNow.Date;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);

            var todayRevenue = invoicesList.Where(i => i.CreatedAt.Date == today).Sum(i => i.TotalAmount);
            var monthRevenue = invoicesList.Where(i => i.CreatedAt.Date >= thisMonthStart).Sum(i => i.TotalAmount);
            var unfilteredTotalRevenue = invoicesList.Sum(i => i.TotalAmount);
            var unfilteredInvoicesCount = invoicesList.Count;

            var filteredInvoices = invoicesList;
            if (fromDate.HasValue)
                filteredInvoices = filteredInvoices.Where(i => i.CreatedAt.Date >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                filteredInvoices = filteredInvoices.Where(i => i.CreatedAt.Date <= toDate.Value.Date).ToList();

            var revenueDtos = filteredInvoices.Select(i => 
            {
                var playingFee = i.TableSession?.TotalPrice ?? 0;
                var serviceFee = Math.Max(0, i.Subtotal - playingFee);
                var playTimeMinutes = i.TableSession != null && i.TableSession.EndTime.HasValue 
                    ? (i.TableSession.EndTime.Value - i.TableSession.StartTime).TotalMinutes 
                    : 0;

                return new RevenueDto
                {
                    InvoiceId = i.Id,
                    TableName = i.TableSession?.BilliardTable?.TableName ?? "Unknown",
                    PaidAt = i.CreatedAt,
                    Subtotal = i.Subtotal,
                    TotalAmount = i.TotalAmount,
                    PaymentMethod = i.PaymentMethod.ToString(),
                    StartTime = i.TableSession?.StartTime ?? DateTime.MinValue,
                    EndTime = i.TableSession?.EndTime,
                    PlayTimeMinutes = playTimeMinutes,
                    PlayingFee = playingFee,
                    ServiceFee = serviceFee
                };
            }).ToList();

            return new PersonalRevenueDto
            {
                StaffId = userId,
                StaffName = user.FullName,
                TotalRevenue = revenueDtos.Sum(r => r.TotalAmount),
                InvoicesCount = revenueDtos.Count,
                TodayRevenue = todayRevenue,
                MonthRevenue = monthRevenue,
                UnfilteredTotalRevenue = unfilteredTotalRevenue,
                UnfilteredInvoicesCount = unfilteredInvoicesCount,
                Revenues = revenueDtos
            };
        }

        public async Task<PersonalRevenueDto> GetRevenueByStaffAsync(Guid staffId, DateTime? fromDate, DateTime? toDate)
        {
            return await GetPersonalRevenueAsync(staffId, fromDate, toDate);
        }

        public async Task<RevenueSummaryDto> GetTotalRevenueAsync(RevenueFilterQuery filter)
        {
            var query = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                includeProperties: "TableSession,TableSession.User"
            );
            var invoices = query.ToList();

            var today = DateTime.UtcNow.Date;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);

            // Compute overall stats (unfiltered by date, but filtered by staff if staff filter is selected)
            var cardInvoices = invoices;
            if (filter.StaffId.HasValue)
            {
                cardInvoices = cardInvoices.Where(i => i.TableSession != null && i.TableSession.UserId == filter.StaffId.Value).ToList();
            }

            var todayInvoices = cardInvoices.Where(i => i.CreatedAt.Date == today).ToList();
            var todayRevenue = todayInvoices.Sum(i => i.TotalAmount);
            var todayInvoicesCount = todayInvoices.Count;
            var monthRevenue = cardInvoices.Where(i => i.CreatedAt.Date >= thisMonthStart).Sum(i => i.TotalAmount);

            // Now apply full filters (including dates)
            var filteredInvoices = invoices;
            if (filter.FromDate.HasValue)
                filteredInvoices = filteredInvoices.Where(i => i.CreatedAt.Date >= filter.FromDate.Value.Date).ToList();
            if (filter.ToDate.HasValue)
                filteredInvoices = filteredInvoices.Where(i => i.CreatedAt.Date <= filter.ToDate.Value.Date).ToList();
            if (filter.StaffId.HasValue)
                filteredInvoices = filteredInvoices.Where(i => i.TableSession != null && i.TableSession.UserId == filter.StaffId.Value).ToList();

            var staffGroups = filteredInvoices.Where(i => i.TableSession != null && i.TableSession.User != null)
                                   .GroupBy(i => i.TableSession!.UserId)
                                   .ToList();

            var staffRevenues = staffGroups.Select(g => new StaffRevenueDto
            {
                StaffId = g.Key,
                StaffName = g.First().TableSession!.User!.FullName,
                TotalRevenue = g.Sum(i => i.TotalAmount),
                InvoicesCount = g.Count()
            }).ToList();

            var dailyGroups = filteredInvoices.GroupBy(i => i.CreatedAt.Date)
                                   .Select(g => new DailyRevenueDto
                                   {
                                       Date = g.Key,
                                       TotalRevenue = g.Sum(i => i.TotalAmount)
                                   }).OrderBy(d => d.Date).ToList();

            return new RevenueSummaryDto
            {
                TotalRevenue = filteredInvoices.Sum(i => i.TotalAmount),
                TotalInvoices = filteredInvoices.Count(),
                TodayRevenue = todayRevenue,
                TodayInvoices = todayInvoicesCount,
                MonthRevenue = monthRevenue,
                StaffRevenues = staffRevenues,
                DailyRevenues = dailyGroups
            };
        }
    }
}
