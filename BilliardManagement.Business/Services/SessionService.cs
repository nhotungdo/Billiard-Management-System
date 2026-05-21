using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace BilliardManagement.Business.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingService _billingService;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBillingService billingService,
            ILogger<SessionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingService = billingService;
            _logger = logger;
        }

        public async Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId, int durationHours)
        {
            if (durationHours < 1 || durationHours > 24)
                throw new CustomException("Duration must be between 1 and 24 hours", 400);

            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(tableId);
            if (table == null) throw new CustomException("Table not found", 404);

            if (table.Status == TableStatus.Playing)
                throw new CustomException("Cannot start session when table is already playing", 400);

            if (table.Status == TableStatus.Maintenance)
                throw new CustomException("Cannot start session on a table under maintenance", 400);

            var activeOnTable = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.TableId == tableId && s.Status == SessionStatus.Active && !s.IsFinished);
            if (activeOnTable.Any())
                throw new CustomException("Table already has an active session", 400);

            if (table.Status != TableStatus.Available && table.Status != TableStatus.Reserved)
                throw new CustomException("Table is not available for a new session", 400);

            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddHours(durationHours);
            var totalPrice = table.HourlyRate * durationHours;

            var session = new TableSession
            {
                TableId = tableId,
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                DurationHours = durationHours,
                DurationMinutes = durationHours * 60,
                RemainingMinutes = durationHours * 60,
                TotalPrice = totalPrice,
                IsFinished = false,
                Status = SessionStatus.Active
            };

            table.Status = TableStatus.Playing;

            await _unitOfWork.Repository<TableSession>().AddAsync(session);
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Session started: sessionId={SessionId}, tableId={TableId}, staffId={StaffId}, durationHours={Hours}, endTime={End}",
                session.Id, tableId, userId, durationHours, endTime);

            return await MapSessionDtoAsync(session, table);
        }

        public async Task<SessionDto> ExtendSessionAsync(Guid sessionId, int additionalMinutes, Guid? staffUserId = null)
        {
            if (additionalMinutes <= 0)
                throw new CustomException("Additional minutes must be greater than zero", 400);

            var allowed = new[] { 30, 60, 120 };
            if (!allowed.Contains(additionalMinutes))
                throw new CustomException("Extension must be 30, 60, or 120 minutes", 400);

            var session = await _unitOfWork.Repository<TableSession>().GetFirstOrDefaultAsync(
                s => s.Id == sessionId, "BilliardTable,Orders");
            if (session == null) throw new CustomException("Session not found", 404);
            if (session.Status != SessionStatus.Active || session.IsFinished)
                throw new CustomException("Cannot extend: session is not active", 400);

            var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
            if (table == null) throw new CustomException("Table not found", 404);

            EnsureSessionEndTime(session);
            var oldEnd = session.EndTime!.Value;
            session.EndTime = oldEnd.AddMinutes(additionalMinutes);
            session.DurationMinutes = (session.DurationMinutes ?? session.DurationHours * 60) + additionalMinutes;
            session.DurationHours = (int)Math.Ceiling(session.DurationMinutes.Value / 60.0);
            session.RemainingMinutes = Math.Max(0, (int)Math.Ceiling((session.EndTime.Value - DateTime.UtcNow).TotalMinutes));
            session.TotalPrice += table.HourlyRate * (additionalMinutes / 60m);

            _unitOfWork.Repository<TableSession>().Update(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Session extended: sessionId={SessionId}, staffId={StaffId}, addedMinutes={Minutes}, newEndTime={End}",
                sessionId, staffUserId, additionalMinutes, session.EndTime);

            return await MapSessionDtoAsync(session, table);
        }

        public async Task<SessionDto> EndSessionAsync(Guid sessionId, GenerateBillDto? billDto = null, Guid? staffUserId = null)
        {
            var session = await _unitOfWork.Repository<TableSession>().GetFirstOrDefaultAsync(
                s => s.Id == sessionId, "BilliardTable,Orders");
            if (session == null) throw new CustomException("Session not found", 404);
            if (session.Status != SessionStatus.Active || session.IsFinished)
                throw new CustomException("Cannot end: session is not active or already finished", 400);

            var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
            if (table == null) throw new CustomException("Table not found", 404);

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Finished;
            session.IsFinished = true;
            session.RemainingMinutes = 0;

            if (session.DurationHours <= 0)
                session.DurationHours = Math.Max(1, (int)Math.Ceiling((session.EndTime.Value - session.StartTime).TotalHours));
            session.DurationMinutes = (int)Math.Ceiling((session.EndTime.Value - session.StartTime).TotalMinutes);

            table.Status = TableStatus.Available;
            _unitOfWork.Repository<TableSession>().Update(session);
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            var generateDto = billDto ?? new GenerateBillDto { Discount = 0, PaymentMethod = PaymentMethod.Cash };
            await _billingService.GenerateBillAsync(sessionId, generateDto);

            _logger.LogInformation(
                "Session ended: sessionId={SessionId}, tableId={TableId}, staffId={StaffId}, endTime={End}, tablePrice={Price}",
                sessionId, session.TableId, staffUserId, session.EndTime, session.TotalPrice);

            return await MapSessionDtoAsync(session, table);
        }

        public async Task<IEnumerable<SessionDto>> GetActiveSessionsAsync()
        {
            var sessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.Status == SessionStatus.Active && !s.IsFinished, "BilliardTable,Orders");

            var result = new List<SessionDto>();
            foreach (var session in sessions)
            {
                await RepairSessionIfNeededAsync(session);
                var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
                if (table != null)
                    result.Add(await MapSessionDtoAsync(session, table));
            }
            return result;
        }

        public async Task<IEnumerable<TableDashboardDto>> GetTableDashboardAsync()
        {
            var tables = await _unitOfWork.Repository<BilliardTable>().GetAllAsync();
            var activeSessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.Status == SessionStatus.Active && !s.IsFinished, "BilliardTable,Orders");

            foreach (var session in activeSessions)
                await RepairSessionIfNeededAsync(session);

            var sessionByTable = activeSessions.GroupBy(s => s.TableId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.StartTime).First());

            var dashboard = new List<TableDashboardDto>();
            foreach (var table in tables.Where(t => t.IsActive))
            {
                sessionByTable.TryGetValue(table.Id, out var session);
                SessionDto? sessionDto = null;
                if (session != null)
                    sessionDto = await MapSessionDtoAsync(session, table);

                dashboard.Add(new TableDashboardDto
                {
                    Id = table.Id,
                    TableName = table.TableName,
                    TableType = table.TableType,
                    Status = table.Status,
                    HourlyRate = table.HourlyRate,
                    ActiveSession = sessionDto
                });
            }
            return dashboard.OrderBy(d => d.TableName);
        }

        public SessionRealtimeDto BuildRealtimeDto(TableSession session, BilliardTable table, decimal ordersTotal = 0)
        {
            EnsureSessionEndTime(session);
            var now = DateTime.UtcNow;
            var end = session.EndTime!.Value;
            var remainingSeconds = Math.Max(0, (int)(end - now).TotalSeconds);
            var remainingMinutes = remainingSeconds / 60;
            var isExpired = remainingSeconds <= 0 && session.Status == SessionStatus.Active && !session.IsFinished;

            string timerLevel = "ok";
            if (isExpired) timerLevel = "expired";
            else if (remainingMinutes < 15) timerLevel = "warning";

            return new SessionRealtimeDto
            {
                SessionId = session.Id,
                TableId = table.Id,
                TableName = table.TableName,
                Status = (int)session.Status,
                TableStatus = (int)table.Status,
                StartTime = AsUtc(session.StartTime),
                EndTime = end,
                DurationHours = session.DurationHours,
                RemainingMinutes = remainingMinutes,
                RemainingSeconds = remainingSeconds,
                TotalPrice = session.TotalPrice,
                OrdersTotal = ordersTotal,
                CurrentTotal = session.TotalPrice + ordersTotal,
                IsExpired = isExpired,
                IsFinished = session.IsFinished,
                TimerLevel = timerLevel
            };
        }

        private async Task<SessionDto> MapSessionDtoAsync(TableSession session, BilliardTable table)
        {
            await RepairSessionIfNeededAsync(session);
            EnsureSessionEndTime(session);

            var (ordersTotal, orderLines) = await LoadOrderSummaryAsync(session.Id);
            var end = session.EndTime!.Value;
            var now = DateTime.UtcNow;
            var remainingSeconds = Math.Max(0, (int)(end - now).TotalSeconds);
            var remainingMinutes = remainingSeconds / 60;
            var isExpired = remainingSeconds <= 0 && session.Status == SessionStatus.Active && !session.IsFinished;

            session.RemainingMinutes = remainingMinutes;

            return new SessionDto
            {
                Id = session.Id,
                TableId = session.TableId,
                UserId = session.UserId,
                TableName = table.TableName,
                TableType = table.TableType,
                HourlyRate = table.HourlyRate,
                StartTime = AsUtc(session.StartTime),
                EndTime = end,
                DurationHours = session.DurationHours > 0 ? session.DurationHours : Math.Max(1, (int)Math.Ceiling((end - AsUtc(session.StartTime)).TotalHours)),
                DurationMinutes = session.DurationMinutes,
                RemainingMinutes = remainingMinutes,
                RemainingSeconds = remainingSeconds,
                TotalPrice = session.TotalPrice,
                OrdersTotal = ordersTotal,
                CurrentTotal = session.TotalPrice + ordersTotal,
                IsFinished = session.IsFinished,
                IsExpired = isExpired,
                Status = session.Status,
                OrderLines = orderLines
            };
        }

        private async Task RepairSessionIfNeededAsync(TableSession session)
        {
            var needsSave = false;

            if (session.DurationHours <= 0)
            {
                session.DurationHours = 2;
                needsSave = true;
            }

            if (!session.EndTime.HasValue)
            {
                session.EndTime = AsUtc(session.StartTime).AddHours(session.DurationHours);
                session.DurationMinutes = session.DurationHours * 60;
                if (session.TotalPrice <= 0)
                {
                    var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
                    if (table != null)
                        session.TotalPrice = table.HourlyRate * session.DurationHours;
                }
                needsSave = true;
            }

            if (needsSave && session.Status == SessionStatus.Active && !session.IsFinished)
            {
                _unitOfWork.Repository<TableSession>().Update(session);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Repaired session {SessionId}: EndTime={End}, DurationHours={Hours}",
                    session.Id, session.EndTime, session.DurationHours);
            }
        }

        private static void EnsureSessionEndTime(TableSession session)
        {
            if (!session.EndTime.HasValue)
            {
                var hours = session.DurationHours > 0 ? session.DurationHours : 1;
                session.EndTime = AsUtc(session.StartTime).AddHours(hours);
            }
            else
            {
                session.EndTime = AsUtc(session.EndTime.Value);
            }

            session.StartTime = AsUtc(session.StartTime);
        }

        private async Task<(decimal ordersTotal, List<SessionOrderLineDto> lines)> LoadOrderSummaryAsync(Guid sessionId)
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                o => o.TableSessionId == sessionId, "OrderItems,OrderItems.Product");

            var lines = new List<SessionOrderLineDto>();
            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    var lineTotal = item.TotalPrice > 0 ? item.TotalPrice : item.Quantity * item.UnitPrice;
                    lines.Add(new SessionOrderLineDto
                    {
                        ProductName = item.Product?.ProductName ?? "Sản phẩm",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = lineTotal
                    });
                }
            }

            var grouped = lines
                .GroupBy(l => l.ProductName)
                .Select(g => new SessionOrderLineDto
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().UnitPrice,
                    LineTotal = g.Sum(x => x.LineTotal)
                })
                .ToList();

            return (orders.Sum(o => o.TotalAmount), grouped);
        }

        private static DateTime AsUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }
    }
}
