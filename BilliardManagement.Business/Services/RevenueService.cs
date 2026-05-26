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
                i => i.TableSession != null && i.TableSession.UserId == userId && i.IsPaid,
                includeProperties: "TableSession,TableSession.BilliardTable"
            );

            if (fromDate.HasValue)
                invoices = invoices.Where(i => i.CreatedAt.Date >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                invoices = invoices.Where(i => i.CreatedAt.Date <= toDate.Value.Date).ToList();

            var revenueDtos = invoices.Select(i => new RevenueDto
            {
                InvoiceId = i.Id,
                TableName = i.TableSession?.BilliardTable?.TableName ?? "Unknown",
                PaidAt = i.CreatedAt,
                Subtotal = i.Subtotal,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount,
                PaymentMethod = i.PaymentMethod.ToString()
            }).ToList();

            return new PersonalRevenueDto
            {
                StaffId = userId,
                StaffName = user.FullName,
                TotalRevenue = revenueDtos.Sum(r => r.TotalAmount),
                InvoicesCount = revenueDtos.Count,
                Revenues = revenueDtos
            };
        }

        public async Task<PersonalRevenueDto> GetRevenueByStaffAsync(Guid staffId, DateTime? fromDate, DateTime? toDate)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(staffId);
            if (user == null) throw new Exception("User not found");

            var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                i => i.TableSession != null && i.TableSession.UserId == staffId && i.IsPaid,
                includeProperties: "TableSession,TableSession.BilliardTable"
            );

            if (fromDate.HasValue)
                invoices = invoices.Where(i => i.CreatedAt.Date >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                invoices = invoices.Where(i => i.CreatedAt.Date <= toDate.Value.Date).ToList();

            var revenueDtos = invoices.Select(i => new RevenueDto
            {
                InvoiceId = i.Id,
                TableName = i.TableSession?.BilliardTable?.TableName ?? "Unknown",
                PaidAt = i.CreatedAt,
                Subtotal = i.Subtotal,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount,
                PaymentMethod = i.PaymentMethod.ToString()
            }).ToList();

            return new PersonalRevenueDto
            {
                StaffId = staffId,
                StaffName = user.FullName,
                TotalRevenue = revenueDtos.Sum(i => i.TotalAmount),
                InvoicesCount = revenueDtos.Count,
                Revenues = revenueDtos
            };
        }

        public async Task<RevenueSummaryDto> GetTotalRevenueAsync(RevenueFilterQuery filter)
        {
            var query = await _unitOfWork.Repository<Invoice>().GetAllAsync(
                i => i.IsPaid,
                includeProperties: "TableSession,TableSession.User"
            );

            if (filter.FromDate.HasValue)
                query = query.Where(i => i.CreatedAt.Date >= filter.FromDate.Value.Date).ToList();
            if (filter.ToDate.HasValue)
                query = query.Where(i => i.CreatedAt.Date <= filter.ToDate.Value.Date).ToList();
            if (filter.StaffId.HasValue)
                query = query.Where(i => i.TableSession != null && i.TableSession.UserId == filter.StaffId.Value).ToList();

            var staffGroups = query.Where(i => i.TableSession != null && i.TableSession.User != null)
                                   .GroupBy(i => i.TableSession!.UserId)
                                   .ToList();

            var staffRevenues = staffGroups.Select(g => new StaffRevenueDto
            {
                StaffId = g.Key,
                StaffName = g.First().TableSession!.User!.FullName,
                TotalRevenue = g.Sum(i => i.TotalAmount),
                InvoicesCount = g.Count()
            }).ToList();

            var dailyGroups = query.GroupBy(i => i.CreatedAt.Date)
                                   .Select(g => new DailyRevenueDto
                                   {
                                       Date = g.Key,
                                       TotalRevenue = g.Sum(i => i.TotalAmount)
                                   }).OrderBy(d => d.Date).ToList();

            return new RevenueSummaryDto
            {
                TotalRevenue = query.Sum(i => i.TotalAmount),
                TotalInvoices = query.Count(),
                StaffRevenues = staffRevenues,
                DailyRevenues = dailyGroups
            };
        }
    }
}
