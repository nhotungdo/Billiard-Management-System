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

using BilliardManagement.Common.Responses;

namespace BilliardManagement.Business.Services
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingService _billingService;
        private readonly ILogger<SessionService> _logger;
        private readonly ICustomerService _customerService;

        public SessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBillingService billingService,
            ILogger<SessionService> logger,
            ICustomerService customerService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingService = billingService;
            _logger = logger;
            _customerService = customerService;
        }

        public async Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId, int durationHours, string? customerName = null, string? customerPhone = null, int paymentMethod = 0, Guid? comboId = null)
        {
            if (durationHours < 0 || durationHours > 24)
                throw new CustomException("Duration must be between 0 and 24 hours", 400);

            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(tableId);
            if (table == null) throw new CustomException("Table not found", 404);

            if (table.Status == TableStatus.Maintenance)
                throw new CustomException($"Không thể mở bàn '{table.TableName}' vì đang ở trạng thái bảo trì.", 400);

            Combo? combo = null;
            if (comboId.HasValue)
            {
                combo = await _unitOfWork.Repository<Combo>().GetFirstOrDefaultAsync(
                    c => c.Id == comboId.Value && !c.IsDeleted && c.IsActive,
                    "ComboItems,ComboItems.Product");
                if (combo == null)
                    throw new CustomException("Combo không tồn tại hoặc đã bị ẩn", 400);

                var isVipTable = table.TableType.Contains("VIP", StringComparison.OrdinalIgnoreCase);
                var isVipCombo = combo.IsVip || combo.Name.Contains("VIP", StringComparison.OrdinalIgnoreCase) || combo.ComboCode.Contains("VIP", StringComparison.OrdinalIgnoreCase);
                if (!isVipCombo && isVipTable)
                {
                    throw new CustomException($"Gói combo thường '{combo.Name}' không được áp dụng cho Bàn VIP ({table.TableName}). Vui lòng chọn Combo VIP!", 400);
                }
                if (isVipCombo && !isVipTable)
                {
                    throw new CustomException($"Gói Combo VIP '{combo.Name}' chỉ áp dụng cho Bàn VIP ({table.TableName}). Vui lòng chọn Bàn VIP!", 400);
                }
            }

            var activeOnTable = (await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.TableId == tableId && s.Status == SessionStatus.Active && !s.IsFinished)).ToList();

            if (table.Status == TableStatus.Playing)
            {
                if (activeOnTable.Any())
                {
                    throw new CustomException($"Bàn '{table.TableName}' đang trong phiên chơi hoạt động. Vui lòng kết thúc bàn trước khi tạo phiên chơi mới!", 400);
                }
                else
                {
                    // Auto-heal table status if DB status was left as Playing but no active session exists
                    table.Status = TableStatus.Available;
                }
            }
            else if (activeOnTable.Any())
            {
                if (table.Status == TableStatus.Available || table.Status == TableStatus.Waiting)
                {
                    // Auto-heal orphaned active sessions because table state is Available/Waiting
                    foreach (var s in activeOnTable)
                    {
                        s.EndTime = s.EndTime ?? DateTime.UtcNow;
                        s.Status = SessionStatus.Finished;
                        s.IsFinished = true;
                        s.RemainingMinutes = 0;
                        _unitOfWork.Repository<TableSession>().Update(s);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            if (table.Status == TableStatus.Playing)
            {
                // Auto-heal table status if DB status was left as Playing but no active session exists
                table.Status = TableStatus.Available;
                _unitOfWork.Repository<BilliardTable>().Update(table);
                await _unitOfWork.SaveChangesAsync();
            }

            if (table.Status != TableStatus.Available && table.Status != TableStatus.Waiting)
                throw new CustomException("Table is not available for a new session", 400);

            var startTime = DateTime.UtcNow;
            int comboMins = combo != null ? combo.PlayingHours * 60 : 0;
            DateTime? comboEndTime = combo != null && comboMins > 0 ? startTime.AddMinutes(comboMins) : null;
            DateTime? endTime = comboEndTime ?? (durationHours > 0 ? startTime.AddHours(durationHours) : null);
            decimal comboPrice = combo != null ? combo.Price : 0;
            decimal totalPrice = combo != null ? 0 : (durationHours > 0 ? table.HourlyRate * durationHours : 0);

            CustomerDto? customerDto = null;
            Guid? customerId = null;
            if (!string.IsNullOrWhiteSpace(customerPhone))
            {
                var cleanedPhone = customerPhone.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedPhone, @"^0\d{9}$"))
                {
                    throw new CustomException("Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).", 400);
                }

                if (!string.IsNullOrWhiteSpace(customerName))
                {
                    customerDto = await _customerService.FindOrCreateCustomerAsync(customerName.Trim(), cleanedPhone);
                    customerId = customerDto.Id;
                }
            }

            var session = new TableSession
            {
                TableId = tableId,
                UserId = userId,
                CustomerId = customerId,
                StartTime = startTime,
                EndTime = endTime,
                DurationHours = combo != null ? combo.PlayingHours : durationHours,
                DurationMinutes = comboMins > 0 ? comboMins : (durationHours > 0 ? durationHours * 60 : (int?)null),
                RemainingMinutes = comboMins > 0 ? comboMins : (durationHours > 0 ? durationHours * 60 : 0),
                TotalPrice = totalPrice,
                IsFinished = false,
                Status = SessionStatus.Active,
                ComboId = combo?.Id,
                ComboHours = combo?.PlayingHours ?? 0,
                ComboDurationMinutes = comboMins,
                ComboEndTime = comboEndTime,
                ComboPrice = comboPrice
            };

            table.Status = TableStatus.Playing;

            await _unitOfWork.Repository<TableSession>().AddAsync(session);

            if (combo != null)
            {
                var sessionCombo = new SessionCombo
                {
                    Id = Guid.NewGuid(),
                    TableSessionId = session.Id,
                    ComboId = combo.Id,
                    ComboName = combo.Name,
                    Price = combo.Price,
                    DurationMinutes = comboMins,
                    AppliedAt = startTime
                };
                await _unitOfWork.Repository<SessionCombo>().AddAsync(sessionCombo);

                if (combo.ComboItems != null && combo.ComboItems.Any())
                {
                    var comboOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = session.Id,
                        OrderedBy = userId,
                        OrderTime = startTime,
                        TotalAmount = 0,
                        Status = OrderStatus.Completed,
                        IsComboOrder = true
                    };

                    foreach (var ci in combo.ComboItems)
                    {
                        var product = ci.Product ?? await _unitOfWork.Repository<Product>().GetByIdAsync(ci.ProductId);
                        if (product != null)
                        {
                            if (product.StockQuantity >= ci.Quantity)
                            {
                                product.StockQuantity -= ci.Quantity;
                                _unitOfWork.Repository<Product>().Update(product);
                            }

                            comboOrder.OrderItems.Add(new OrderItem
                            {
                                Id = Guid.NewGuid(),
                                OrderId = comboOrder.Id,
                                ProductId = ci.ProductId,
                                Quantity = ci.Quantity,
                                UnitPrice = product.Price,
                                TotalPrice = product.Price * ci.Quantity
                            });
                        }
                    }
                    await _unitOfWork.Repository<Order>().AddAsync(comboOrder);
                }
            }

            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            // Create prepaid invoice only for fixed duration prepaid sessions (durationHours > 0 and no combo)
            if (durationHours > 0 && combo == null)
            {
                var enumPaymentMethod = Enum.IsDefined(typeof(PaymentMethod), paymentMethod) 
                    ? (PaymentMethod)paymentMethod 
                    : PaymentMethod.Cash;

                var prepaidInvoice = new Invoice
                {
                    TableSessionId = session.Id,
                    CustomerId = customerId,
                    Subtotal = totalPrice,
                    TotalAmount = totalPrice,
                    PaymentMethod = enumPaymentMethod,
                    IsPaid = true,
                    CreatedAt = DateTime.UtcNow
                };

                if (customerId.HasValue)
                {
                    var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId.Value);
                    if (customer != null)
                    {
                        customer.TotalVisits += 1;
                        customer.TotalPlayHours += (decimal)durationHours;
                        customer.TotalSpent += totalPrice;
                        customer.LastVisitDate = DateTime.UtcNow;
                        if (!customer.FirstVisitDate.HasValue)
                        {
                            customer.FirstVisitDate = DateTime.UtcNow;
                        }
                        _unitOfWork.Repository<Customer>().Update(customer);
                    }
                }

                await _unitOfWork.Repository<Invoice>().AddAsync(prepaidInvoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Prepaid session started & paid: sessionId={SessionId}, tableId={TableId}, staffId={StaffId}, durationHours={Hours}, totalPrice={TotalPrice}, paymentMethod={PaymentMethod}",
                    session.Id, tableId, userId, durationHours, totalPrice, enumPaymentMethod);
            }
            else
            {
                _logger.LogInformation(
                    "Session started: sessionId={SessionId}, tableId={TableId}, staffId={StaffId}, combo={ComboName}",
                    session.Id, tableId, userId, combo?.Name ?? "None");
            }

            var dto = await MapSessionDtoAsync(session, table);
            if (customerDto != null)
            {
                dto.CustomerName = customerDto.FullName;
                dto.CustomerPhone = customerDto.PhoneNumber;
            }
            return dto;
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

            var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);

            if (session.Status != SessionStatus.Active || session.IsFinished)
            {
                // Ensure table status is reset to Available if left as Playing
                if (table != null && table.Status == TableStatus.Playing)
                {
                    table.Status = TableStatus.Available;
                    _unitOfWork.Repository<BilliardTable>().Update(table);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Auto-complete non-cancelled orders for this session
                var existingOrders = await _unitOfWork.Repository<Order>().GetAllAsync(
                    o => o.TableSessionId == sessionId && o.Status != OrderStatus.Cancelled);
                foreach (var order in existingOrders)
                {
                    order.Status = OrderStatus.Completed;
                    _unitOfWork.Repository<Order>().Update(order);
                }
                await _unitOfWork.SaveChangesAsync();

                var generateDtoFallback = billDto ?? new GenerateBillDto { PaymentMethod = PaymentMethod.Cash };
                await _billingService.GenerateBillAsync(sessionId, generateDtoFallback);

                _logger.LogInformation(
                    "Session already ended/inactive, handled idempotently: sessionId={SessionId}, tableId={TableId}",
                    sessionId, session.TableId);

                return await MapSessionDtoAsync(session, table ?? new BilliardTable());
            }

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Finished;
            session.IsFinished = true;
            session.RemainingMinutes = 0;

            var totalSeconds = Math.Max(1, (int)(session.EndTime.Value - AsUtc(session.StartTime)).TotalSeconds);
            var elapsedMinutes = (int)Math.Ceiling(totalSeconds / 60.0);

            var sessionCombos = await _unitOfWork.Repository<SessionCombo>().GetAllAsync(sc => sc.TableSessionId == session.Id);
            int totalComboMins = sessionCombos.Any() ? sessionCombos.Sum(sc => sc.DurationMinutes) : (session.ComboDurationMinutes > 0 ? session.ComboDurationMinutes : session.ComboHours * 60);

            if (totalComboMins > 0 || session.ComboEndTime.HasValue || session.ComboId.HasValue)
            {
                DateTime comboEndTime = session.ComboEndTime ?? AsUtc(session.StartTime).AddMinutes(totalComboMins);
                if (session.EndTime.Value > comboEndTime)
                {
                    var overSeconds = (session.EndTime.Value - comboEndTime).TotalSeconds;
                    session.TotalPrice = Math.Round((decimal)overSeconds / 3600m * (table?.HourlyRate ?? 0));
                }
                else
                {
                    session.TotalPrice = 0;
                }
                session.DurationMinutes = elapsedMinutes;
                session.DurationHours = Math.Max(1, (int)Math.Ceiling(elapsedMinutes / 60.0));
            }
            else if (session.DurationHours <= 0)
            {
                var hourlyRate = table?.HourlyRate ?? 0;
                var calculatedFee = Math.Round((decimal)totalSeconds / 3600m * hourlyRate);
                var actualPlayFee = Math.Max(hourlyRate, calculatedFee);
                session.TotalPrice = actualPlayFee;
                session.DurationMinutes = elapsedMinutes;
                session.DurationHours = Math.Max(1, (int)Math.Ceiling(elapsedMinutes / 60.0));
            }
            else
            {
                session.DurationMinutes = (int)Math.Ceiling((session.EndTime.Value - session.StartTime).TotalMinutes);
            }

            if (table != null)
            {
                table.Status = TableStatus.Available;
                _unitOfWork.Repository<BilliardTable>().Update(table);
            }

            // Clean up any other orphaned active sessions for this table to prevent blocking future sessions
            var otherActiveOnTable = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.TableId == session.TableId && s.Id != sessionId && s.Status == SessionStatus.Active && !s.IsFinished);
            foreach (var otherSession in otherActiveOnTable)
            {
                otherSession.EndTime = otherSession.EndTime ?? DateTime.UtcNow;
                otherSession.Status = SessionStatus.Finished;
                otherSession.IsFinished = true;
                otherSession.RemainingMinutes = 0;
                _unitOfWork.Repository<TableSession>().Update(otherSession);
            }

            // Auto-complete all non-cancelled orders for this session
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                o => o.TableSessionId == sessionId && o.Status != OrderStatus.Cancelled);
            foreach (var order in orders)
            {
                order.Status = OrderStatus.Completed;
                _unitOfWork.Repository<Order>().Update(order);
            }

            _unitOfWork.Repository<TableSession>().Update(session);
            await _unitOfWork.SaveChangesAsync();

            var generateDto = billDto ?? new GenerateBillDto { PaymentMethod = PaymentMethod.Cash };
            await _billingService.GenerateBillAsync(sessionId, generateDto);

            _logger.LogInformation(
                "Session ended: sessionId={SessionId}, tableId={TableId}, staffId={StaffId}, endTime={End}, tablePrice={Price}",
                sessionId, session.TableId, staffUserId, session.EndTime, session.TotalPrice);

            return await MapSessionDtoAsync(session, table ?? new BilliardTable());
        }

        public async Task<IEnumerable<SessionDto>> GetActiveSessionsAsync()
        {
            var sessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.Status == SessionStatus.Active && !s.IsFinished, "BilliardTable,Orders,Customer");

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
                s => s.Status == SessionStatus.Active && !s.IsFinished, "BilliardTable,Orders,Customer");

            foreach (var session in activeSessions)
                await RepairSessionIfNeededAsync(session);

            var sessionByTable = activeSessions.GroupBy(s => s.TableId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.StartTime).First());

            var dashboard = new List<TableDashboardDto>();
            var hasStatusUpdates = false;
            foreach (var table in tables.Where(t => t.IsActive))
            {
                sessionByTable.TryGetValue(table.Id, out var session);
                SessionDto? sessionDto = null;
                if (session != null)
                {
                    sessionDto = await MapSessionDtoAsync(session, table);
                    if (table.Status != TableStatus.Playing && table.Status != TableStatus.Maintenance)
                    {
                        table.Status = TableStatus.Playing;
                        _unitOfWork.Repository<BilliardTable>().Update(table);
                        hasStatusUpdates = true;
                    }
                }
                else
                {
                    if (table.Status == TableStatus.Playing)
                    {
                        table.Status = TableStatus.Available;
                        _unitOfWork.Repository<BilliardTable>().Update(table);
                        hasStatusUpdates = true;
                    }
                }

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

            if (hasStatusUpdates)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return dashboard.OrderBy(d => d.TableName);
        }

        public SessionRealtimeDto BuildRealtimeDto(TableSession session, BilliardTable table, decimal ordersTotal = 0)
        {
            var sessionCombos = _unitOfWork.Repository<SessionCombo>().GetAllAsync(sc => sc.TableSessionId == session.Id).GetAwaiter().GetResult();
            var appliedCombos = _mapper.Map<List<SessionComboDto>>(sessionCombos.OrderBy(sc => sc.AppliedAt).ToList());

            decimal comboPrice = sessionCombos.Any() ? sessionCombos.Sum(sc => sc.Price) : session.ComboPrice;
            int comboDurationMinutes = sessionCombos.Any() ? sessionCombos.Sum(sc => sc.DurationMinutes) : (session.ComboDurationMinutes > 0 ? session.ComboDurationMinutes : session.ComboHours * 60);
            DateTime? comboEndTime = session.ComboEndTime;

            if (!comboEndTime.HasValue && comboDurationMinutes > 0)
            {
                comboEndTime = AsUtc(session.StartTime).AddMinutes(comboDurationMinutes);
            }

            bool hasCombo = comboDurationMinutes > 0 || comboEndTime.HasValue || session.ComboId.HasValue || sessionCombos.Any();

            decimal tableFeeAfterCombo = 0;
            bool isUsingCombo = false;
            bool isOverComboTime = false;
            int overComboMinutes = 0;
            int remainingSeconds = 0;
            int remainingMinutes = 0;
            bool isExpired = false;
            string timerLevel = "ok";
            decimal playFee = 0;

            var effectiveNow = (session.IsFinished && session.EndTime.HasValue) ? AsUtc(session.EndTime.Value) : DateTime.UtcNow;

            if (hasCombo && comboEndTime.HasValue)
            {
                isUsingCombo = true;
                if (effectiveNow <= comboEndTime.Value)
                {
                    tableFeeAfterCombo = 0;
                    isOverComboTime = false;
                    overComboMinutes = 0;
                    remainingSeconds = Math.Max(0, (int)(comboEndTime.Value - effectiveNow).TotalSeconds);
                    remainingMinutes = remainingSeconds / 60;
                    isExpired = false;
                    timerLevel = "ok";
                }
                else
                {
                    isOverComboTime = true;
                    var overSeconds = (effectiveNow - comboEndTime.Value).TotalSeconds;
                    overComboMinutes = (int)Math.Ceiling(overSeconds / 60.0);
                    tableFeeAfterCombo = Math.Round((decimal)overSeconds / 3600m * table.HourlyRate);
                    remainingSeconds = 0;
                    remainingMinutes = 0;
                    isExpired = true;
                    timerLevel = "expired";
                }
                playFee = tableFeeAfterCombo;
            }
            else if (session.DurationHours == 0 || !session.EndTime.HasValue)
            {
                var elapsedSec = Math.Max(0, (int)(effectiveNow - AsUtc(session.StartTime)).TotalSeconds);
                remainingSeconds = elapsedSec;
                remainingMinutes = elapsedSec / 60;
                var calculatedFee = Math.Round((decimal)elapsedSec / 3600m * table.HourlyRate);
                playFee = Math.Max(table.HourlyRate, calculatedFee);
                tableFeeAfterCombo = playFee;
                isExpired = false;
                timerLevel = "ok";
            }
            else
            {
                EnsureSessionEndTime(session);
                var endVal = session.EndTime!.Value;
                remainingSeconds = Math.Max(0, (int)(endVal - effectiveNow).TotalSeconds);
                remainingMinutes = remainingSeconds / 60;
                playFee = session.TotalPrice > 0 ? session.TotalPrice : table.HourlyRate * session.DurationHours;
                tableFeeAfterCombo = playFee;
                isExpired = remainingSeconds <= 0 && session.Status == SessionStatus.Active && !session.IsFinished;
                if (isExpired) timerLevel = "expired";
                else if (remainingMinutes < 15) timerLevel = "warning";
            }

            return new SessionRealtimeDto
            {
                SessionId = session.Id,
                TableId = table.Id,
                TableName = table.TableName,
                Status = (int)session.Status,
                TableStatus = (int)table.Status,
                StartTime = AsUtc(session.StartTime),
                EndTime = comboEndTime ?? session.EndTime,
                DurationHours = session.DurationHours,
                RemainingMinutes = remainingMinutes,
                RemainingSeconds = remainingSeconds,
                TotalPrice = playFee,
                OrdersTotal = ordersTotal,
                CurrentTotal = comboPrice + ordersTotal + playFee,
                IsExpired = isExpired,
                IsFinished = session.IsFinished,
                TimerLevel = timerLevel,
                CustomerName = session.Customer?.FullName,
                CustomerPhone = session.Customer?.PhoneNumber,
                ComboId = session.ComboId,
                ComboHours = session.ComboHours,
                ComboDurationMinutes = comboDurationMinutes,
                ComboEndTime = comboEndTime,
                ComboPrice = comboPrice,
                IsUsingCombo = isUsingCombo,
                IsOverComboTime = isOverComboTime,
                OverComboMinutes = overComboMinutes,
                TableFeeAfterCombo = tableFeeAfterCombo,
                AppliedCombos = appliedCombos
            };
        }

        private async Task<SessionDto> MapSessionDtoAsync(TableSession session, BilliardTable table)
        {
            await RepairSessionIfNeededAsync(session);

            var (ordersTotal, orderLines) = await LoadOrderSummaryAsync(session.Id);
            var sessionCombos = await _unitOfWork.Repository<SessionCombo>().GetAllAsync(sc => sc.TableSessionId == session.Id);
            var appliedCombos = _mapper.Map<List<SessionComboDto>>(sessionCombos.OrderBy(sc => sc.AppliedAt).ToList());

            decimal comboPrice = sessionCombos.Any() ? sessionCombos.Sum(sc => sc.Price) : session.ComboPrice;
            int comboDurationMinutes = sessionCombos.Any() ? sessionCombos.Sum(sc => sc.DurationMinutes) : (session.ComboDurationMinutes > 0 ? session.ComboDurationMinutes : session.ComboHours * 60);
            DateTime? comboEndTime = session.ComboEndTime;

            if (!comboEndTime.HasValue && comboDurationMinutes > 0)
            {
                comboEndTime = AsUtc(session.StartTime).AddMinutes(comboDurationMinutes);
            }

            bool hasCombo = comboDurationMinutes > 0 || comboEndTime.HasValue || session.ComboId.HasValue || sessionCombos.Any();

            decimal tableFeeAfterCombo = 0;
            bool isUsingCombo = false;
            bool isOverComboTime = false;
            int overComboMinutes = 0;
            int remainingSeconds = 0;
            int remainingMinutes = 0;
            bool isExpired = false;
            decimal playFee = 0;

            var effectiveNow = (session.IsFinished && session.EndTime.HasValue) ? AsUtc(session.EndTime.Value) : DateTime.UtcNow;

            if (hasCombo && comboEndTime.HasValue)
            {
                isUsingCombo = true;
                if (effectiveNow <= comboEndTime.Value)
                {
                    tableFeeAfterCombo = 0;
                    isOverComboTime = false;
                    overComboMinutes = 0;
                    remainingSeconds = Math.Max(0, (int)(comboEndTime.Value - effectiveNow).TotalSeconds);
                    remainingMinutes = remainingSeconds / 60;
                    isExpired = false;
                }
                else
                {
                    isOverComboTime = true;
                    var overSeconds = (effectiveNow - comboEndTime.Value).TotalSeconds;
                    overComboMinutes = (int)Math.Ceiling(overSeconds / 60.0);
                    tableFeeAfterCombo = Math.Round((decimal)overSeconds / 3600m * table.HourlyRate);
                    remainingSeconds = 0;
                    remainingMinutes = 0;
                    isExpired = true;
                }
                playFee = tableFeeAfterCombo;
            }
            else if (session.DurationHours == 0 || !session.EndTime.HasValue)
            {
                var elapsedSec = Math.Max(0, (int)(effectiveNow - AsUtc(session.StartTime)).TotalSeconds);
                remainingSeconds = elapsedSec;
                remainingMinutes = elapsedSec / 60;
                var calculatedFee = Math.Round((decimal)elapsedSec / 3600m * table.HourlyRate);
                playFee = Math.Max(table.HourlyRate, calculatedFee);
                tableFeeAfterCombo = playFee;
                isExpired = false;
            }
            else
            {
                EnsureSessionEndTime(session);
                var endVal = session.EndTime!.Value;
                remainingSeconds = Math.Max(0, (int)(endVal - effectiveNow).TotalSeconds);
                remainingMinutes = remainingSeconds / 60;
                playFee = session.TotalPrice > 0 ? session.TotalPrice : table.HourlyRate * session.DurationHours;
                tableFeeAfterCombo = playFee;
                isExpired = remainingSeconds <= 0 && session.Status == SessionStatus.Active && !session.IsFinished;
                session.RemainingMinutes = remainingMinutes;
            }

            return new SessionDto
            {
                Id = session.Id,
                TableId = session.TableId,
                UserId = session.UserId,
                TableName = table.TableName,
                TableType = table.TableType,
                CustomerName = session.Customer?.FullName,
                CustomerPhone = session.Customer?.PhoneNumber,
                HourlyRate = table.HourlyRate,
                StartTime = AsUtc(session.StartTime),
                EndTime = comboEndTime ?? session.EndTime,
                DurationHours = session.DurationHours,
                DurationMinutes = session.DurationMinutes,
                RemainingMinutes = remainingMinutes,
                RemainingSeconds = remainingSeconds,
                TotalPrice = playFee,
                OrdersTotal = ordersTotal,
                CurrentTotal = comboPrice + ordersTotal + tableFeeAfterCombo,
                IsFinished = session.IsFinished,
                IsExpired = isExpired,
                Status = session.Status,
                ComboId = session.ComboId,
                ComboHours = session.ComboHours,
                ComboDurationMinutes = comboDurationMinutes,
                ComboEndTime = comboEndTime,
                ComboPrice = comboPrice,
                IsUsingCombo = isUsingCombo,
                IsOverComboTime = isOverComboTime,
                OverComboMinutes = overComboMinutes,
                TableFeeAfterCombo = tableFeeAfterCombo,
                AppliedCombos = appliedCombos,
                OrderLines = orderLines
            };
        }

        private async Task RepairSessionIfNeededAsync(TableSession session)
        {
            var needsSave = false;

            if (session.DurationHours > 0 && !session.EndTime.HasValue && !session.ComboEndTime.HasValue && session.ComboDurationMinutes == 0)
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
            if (session.DurationHours > 0 && !session.EndTime.HasValue)
            {
                session.EndTime = AsUtc(session.StartTime).AddHours(session.DurationHours);
            }
            else if (session.EndTime.HasValue)
            {
                session.EndTime = AsUtc(session.EndTime.Value);
            }

            session.StartTime = AsUtc(session.StartTime);
        }

        private async Task<(decimal ordersTotal, List<SessionOrderLineDto> lines)> LoadOrderSummaryAsync(Guid sessionId)
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                o => o.TableSessionId == sessionId && !o.IsComboOrder && o.Status != OrderStatus.Cancelled, "OrderItems,OrderItems.Product");

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

        public async Task<PagedResult<SessionDto>> GetPagedSessionsAsync(SessionQueryParameters query)
        {
            var filters = new List<System.Linq.Expressions.Expression<System.Func<TableSession, bool>>>();

            if (query.Status.HasValue)
            {
                var statusEnum = (SessionStatus)query.Status.Value;
                filters.Add(s => s.Status == statusEnum);
            }
            if (query.TableId.HasValue)
            {
                filters.Add(s => s.TableId == query.TableId.Value);
            }
            if (query.IsFinished.HasValue)
            {
                filters.Add(s => s.IsFinished == query.IsFinished.Value);
            }

            Func<IQueryable<TableSession>, IOrderedQueryable<TableSession>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("StartTime", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(s => s.StartTime) : q.OrderBy(s => s.StartTime);
                }
                else if (query.SortBy.Equals("EndTime", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(s => s.EndTime) : q.OrderBy(s => s.EndTime);
                }
                else if (query.SortBy.Equals("TotalPrice", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(s => s.TotalPrice) : q.OrderBy(s => s.TotalPrice);
                }
            }
            else
            {
                orderBy = q => q.OrderByDescending(s => s.StartTime);
            }

            var (items, totalCount) = await _unitOfWork.Repository<TableSession>().GetPagedAsync(
                filters: filters,
                orderBy: orderBy,
                includeProperties: "BilliardTable,Orders,Customer",
                page: query.PageNumber,
                pageSize: query.PageSize
            );

            var mappedList = new List<SessionDto>();
            foreach (var session in items)
            {
                var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
                if (table != null)
                {
                    mappedList.Add(await MapSessionDtoAsync(session, table));
                }
            }

            return new PagedResult<SessionDto>(mappedList, query.PageNumber, query.PageSize, totalCount);
        }
    }
}
