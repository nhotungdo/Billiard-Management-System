using BilliardManagement.Business.DTOs;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Common.Tenant;
using BilliardManagement.Data;
using BilliardManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BilliardManagement.Business.Services;

public class SystemReportService(BilliardManagementDbContext db, ITenantContext tenant)
{
    public async Task<List<StoreReportDto>> CompareAsync(Guid[]? storeIds, DateTime from, DateTime to)
    {
        if (!tenant.IsSuperAdmin) throw new CustomException("Chỉ SuperAdmin được xem báo cáo hệ thống", 403);
        from = from.Date;
        to = to.Date;
        if (from > to || (to - from).TotalDays > 365)
            throw new CustomException("Khoảng báo cáo phải hợp lệ và tối đa 366 ngày", 400);
        var end = to.AddDays(1);
        var previous = from.AddDays(-(end - from).TotalDays);
        var today = DateTime.UtcNow.Date;
        var month = new DateTime(today.Year, today.Month, 1);
        var query = db.Stores.AsNoTracking();
        if (storeIds?.Length > 0) query = query.Where(s => storeIds.Contains(s.Id));
        var stores = await query.OrderBy(s => s.Name).ToListAsync();
        if (storeIds?.Any(id => stores.All(s => s.Id != id)) == true)
            throw new CustomException("Cửa hàng không tồn tại", 404);
        var ids = stores.Select(s => s.Id).ToArray();
        // Explicit scope: system reports are independent of the operational store switcher.
        var invoices = await db.Invoices.IgnoreQueryFilters().AsNoTracking()
            .Where(i => i.TableSession != null && ids.Contains(i.TableSession.StoreId) &&
                i.CreatedAt >= (previous < month ? previous : month) &&
                i.CreatedAt < (end > today.AddDays(1) ? end : today.AddDays(1)))
            .Select(i => new { i.TableSession!.StoreId, i.CreatedAt, i.TotalAmount }).ToListAsync();
        var tables = await db.BilliardTables.IgnoreQueryFilters().AsNoTracking()
            .Where(t => ids.Contains(t.StoreId)).GroupBy(t => t.StoreId)
            .Select(g => new { Id = g.Key, Count = g.Count(), Active = g.Count(t => t.Status == TableStatus.Playing) }).ToListAsync();
        var staff = await db.Users.IgnoreQueryFilters().AsNoTracking()
            .Where(u => u.StoreId.HasValue && ids.Contains(u.StoreId.Value) && u.Role == UserRole.Staff)
            .GroupBy(u => u.StoreId).Select(g => new { Id = g.Key, Count = g.Count() }).ToListAsync();
        var sessions = await db.TableSessions.IgnoreQueryFilters().AsNoTracking()
            .Where(s => ids.Contains(s.StoreId) && ((s.StartTime >= from && s.StartTime < end) ||
                (s.StartTime >= today && s.StartTime < today.AddDays(1))))
            .Select(s => new { s.StoreId, s.StartTime }).ToListAsync();
        return stores.Select(s =>
        {
            var rows = invoices.Where(i => i.StoreId == s.Id).ToList();
            var revenue = rows.Where(i => i.CreatedAt >= from && i.CreatedAt < end).Sum(i => i.TotalAmount);
            var prior = rows.Where(i => i.CreatedAt >= previous && i.CreatedAt < from).Sum(i => i.TotalAmount);
            var daily = rows.Where(i => i.CreatedAt >= from && i.CreatedAt < end)
                .GroupBy(i => i.CreatedAt.Date).ToDictionary(g => g.Key, g => g.Sum(i => i.TotalAmount));
            return new StoreReportDto
            {
                StoreId = s.Id, StoreName = s.Name, IsActive = s.IsActive,
                TableCount = tables.FirstOrDefault(t => t.Id == s.Id)?.Count ?? 0,
                ActiveTables = tables.FirstOrDefault(t => t.Id == s.Id)?.Active ?? 0,
                StaffCount = staff.FirstOrDefault(t => t.Id == s.Id)?.Count ?? 0,
                Sessions = sessions.Count(t => t.StoreId == s.Id && t.StartTime >= from && t.StartTime < end),
                SessionsToday = sessions.Count(t => t.StoreId == s.Id && t.StartTime >= today && t.StartTime < today.AddDays(1)),
                Revenue = revenue, MonthRevenue = rows.Where(i => i.CreatedAt >= month && i.CreatedAt < today.AddDays(1)).Sum(i => i.TotalAmount),
                GrowthPercent = prior == 0 ? null : Math.Round((revenue - prior) / prior * 100, 1),
                DailyRevenues = Enumerable.Range(0, (end - from).Days).Select(n => new DailyRevenueDto
                { Date = from.AddDays(n), TotalRevenue = daily.GetValueOrDefault(from.AddDays(n)) }).ToList()
            };
        }).ToList();
    }

    public async Task<SystemOverviewDto> OverviewAsync()
    {
        var today = DateTime.UtcNow.Date;
        var stores = await CompareAsync(null, today.AddDays(-29), today);
        return new SystemOverviewDto
        {
            TotalStores = stores.Count, ActiveTables = stores.Sum(s => s.ActiveTables),
            ActiveUsers = await db.Users.IgnoreQueryFilters().CountAsync(u => u.IsActive),
            TodayRevenue = stores.Sum(s => s.DailyRevenues.Last().TotalRevenue),
            DailyRevenues = stores.SelectMany(s => s.DailyRevenues).GroupBy(d => d.Date)
                .Select(g => new DailyRevenueDto { Date = g.Key, TotalRevenue = g.Sum(d => d.TotalRevenue) }).OrderBy(d => d.Date).ToList(),
            Stores = await CompareAsync(null, today.AddDays(-6), today)
        };
    }
}
