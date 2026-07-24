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

namespace BilliardManagement.Business.Services
{
    public class ComboService : IComboService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ComboService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ComboDto>> GetAllCombosAsync(string? search = null, bool? activeOnly = null)
        {
            var combos = await _unitOfWork.Repository<Combo>().GetAllAsync(
                filter: c => !c.IsDeleted 
                    && (!activeOnly.HasValue || !activeOnly.Value || c.IsActive)
                    && (string.IsNullOrWhiteSpace(search) || c.ComboCode.ToLower().Contains(search.Trim().ToLower()) || c.Name.ToLower().Contains(search.Trim().ToLower())),
                includeProperties: "ComboItems,ComboItems.Product"
            );

            var orderedCombos = combos.OrderByDescending(c => c.CreatedAt).ToList();
            return _mapper.Map<IEnumerable<ComboDto>>(orderedCombos);
        }

        public async Task<ComboDto?> GetComboByIdAsync(Guid id)
        {
            var combo = await _unitOfWork.Repository<Combo>().GetFirstOrDefaultAsync(
                filter: c => c.Id == id && !c.IsDeleted,
                includeProperties: "ComboItems,ComboItems.Product"
            );

            return combo == null ? null : _mapper.Map<ComboDto>(combo);
        }

        public async Task<ComboDto?> GetComboByCodeAsync(string comboCode)
        {
            if (string.IsNullOrWhiteSpace(comboCode)) return null;

            var codeNormalized = comboCode.Trim().ToUpper();
            var combo = await _unitOfWork.Repository<Combo>().GetFirstOrDefaultAsync(
                filter: c => c.ComboCode.ToUpper() == codeNormalized && !c.IsDeleted,
                includeProperties: "ComboItems,ComboItems.Product"
            );

            return combo == null ? null : _mapper.Map<ComboDto>(combo);
        }

        public async Task<ComboDto> CreateComboAsync(CreateComboDto dto)
        {
            var codeNormalized = dto.ComboCode.Trim().ToUpper();
            var existing = await _unitOfWork.Repository<Combo>()
                .AnyAsync(c => c.ComboCode.ToUpper() == codeNormalized && !c.IsDeleted);

            if (existing)
            {
                throw new InvalidOperationException($"Mã combo '{dto.ComboCode}' đã tồn tại trong hệ thống.");
            }

            var combo = new Combo
            {
                Id = Guid.NewGuid(),
                ComboCode = dto.ComboCode.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Price = dto.Price,
                Description = dto.Description,
                PlayingHours = dto.PlayingHours,
                IsVip = dto.IsVip,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var item in dto.Items)
                {
                    combo.ComboItems.Add(new ComboItem
                    {
                        Id = Guid.NewGuid(),
                        ComboId = combo.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
            }

            await _unitOfWork.Repository<Combo>().AddAsync(combo);
            await _unitOfWork.SaveChangesAsync();

            return await GetComboByIdAsync(combo.Id) ?? _mapper.Map<ComboDto>(combo);
        }

        public async Task<ComboDto?> UpdateComboAsync(Guid id, UpdateComboDto dto)
        {
            var combo = await _unitOfWork.Repository<Combo>().GetFirstOrDefaultAsync(
                filter: c => c.Id == id && !c.IsDeleted,
                includeProperties: "ComboItems"
            );

            if (combo == null) return null;

            var codeNormalized = dto.ComboCode.Trim().ToUpper();
            var existingCode = await _unitOfWork.Repository<Combo>()
                .AnyAsync(c => c.Id != id && c.ComboCode.ToUpper() == codeNormalized && !c.IsDeleted);

            if (existingCode)
            {
                throw new InvalidOperationException($"Mã combo '{dto.ComboCode}' đã được sử dụng bởi combo khác.");
            }

            combo.ComboCode = codeNormalized;
            combo.Name = dto.Name.Trim();
            combo.Price = dto.Price;
            combo.Description = dto.Description;
            combo.PlayingHours = dto.PlayingHours;
            combo.IsVip = dto.IsVip;
            combo.IsActive = dto.IsActive;

            // Remove existing combo items
            if (combo.ComboItems.Any())
            {
                _unitOfWork.Repository<ComboItem>().RemoveRange(combo.ComboItems.ToList());
            }

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var item in dto.Items)
                {
                    await _unitOfWork.Repository<ComboItem>().AddAsync(new ComboItem
                    {
                        Id = Guid.NewGuid(),
                        ComboId = combo.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetComboByIdAsync(combo.Id);
        }

        public async Task<bool> ToggleComboStatusAsync(Guid id)
        {
            var combo = await _unitOfWork.Repository<Combo>().GetByIdAsync(id);
            if (combo == null || combo.IsDeleted) return false;

            combo.IsActive = !combo.IsActive;
            _unitOfWork.Repository<Combo>().Update(combo);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteComboAsync(Guid id)
        {
            var combo = await _unitOfWork.Repository<Combo>().GetByIdAsync(id);
            if (combo == null || combo.IsDeleted) return false;

            combo.IsDeleted = true;
            _unitOfWork.Repository<Combo>().Update(combo);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApplyComboResultDto> ApplyComboToSessionAsync(ApplyComboRequestDto request, Guid userId)
        {
            var comboDto = await GetComboByCodeAsync(request.ComboCode);
            if (comboDto == null)
            {
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = $"Không tìm thấy combo với mã '{request.ComboCode}'."
                };
            }

            if (!comboDto.IsActive)
            {
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = $"Gói combo '{comboDto.Name}' hiện đang ngừng hoạt động."
                };
            }

            var session = await _unitOfWork.Repository<TableSession>().GetFirstOrDefaultAsync(
                filter: s => s.Id == request.TableSessionId && !s.IsFinished,
                includeProperties: "BilliardTable"
            );

            if (session == null)
            {
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = "Phiên chơi không tồn tại hoặc đã kết thúc."
                };
            }

            var table = session.BilliardTable ?? await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
            var isVipTable = table != null && table.TableType.Contains("VIP", StringComparison.OrdinalIgnoreCase);
            var isVipCombo = comboDto.IsVip || comboDto.Name.Contains("VIP", StringComparison.OrdinalIgnoreCase) || comboDto.ComboCode.Contains("VIP", StringComparison.OrdinalIgnoreCase);

            if (!isVipCombo && isVipTable)
            {
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = $"Gói combo '{comboDto.Name}' là Gói Combo Thường, không được phép áp dụng cho Bàn VIP ({table?.TableName}). Vui lòng chọn Gói Combo VIP!"
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var comboMins = comboDto.PlayingHours * 60;

                // Record SessionCombo
                var sessionCombo = new SessionCombo
                {
                    Id = Guid.NewGuid(),
                    TableSessionId = session.Id,
                    ComboId = comboDto.Id,
                    ComboName = comboDto.Name,
                    Price = comboDto.Price,
                    DurationMinutes = comboMins,
                    AppliedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SessionCombo>().AddAsync(sessionCombo);

                // Create stock deduction order for bundled combo items
                Guid? orderId = null;
                if (comboDto.Items != null && comboDto.Items.Any())
                {
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = session.Id,
                        OrderedBy = userId,
                        OrderTime = DateTime.UtcNow,
                        TotalAmount = 0, // Bundled items covered by combo price
                        Status = OrderStatus.Completed,
                        IsComboOrder = true
                    };
                    orderId = order.Id;

                    foreach (var item in comboDto.Items)
                    {
                        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                        var unitPrice = product != null ? product.Price : item.ProductPrice;

                        order.OrderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = unitPrice * item.Quantity
                        });

                        if (product != null && product.StockQuantity >= item.Quantity)
                        {
                            product.StockQuantity -= item.Quantity;
                            _unitOfWork.Repository<Product>().Update(product);
                        }
                    }

                    await _unitOfWork.Repository<Order>().AddAsync(order);
                }

                // Update combo metadata on session
                if (!session.ComboId.HasValue)
                {
                    session.ComboId = comboDto.Id;
                }
                session.ComboHours += comboDto.PlayingHours;
                session.ComboDurationMinutes += comboMins;
                session.ComboPrice += comboDto.Price;

                // Update ComboEndTime (Req 4 & 5: ComboEndTime += ComboDuration)
                if (!session.ComboEndTime.HasValue)
                {
                    session.ComboEndTime = session.StartTime.AddMinutes(session.ComboDurationMinutes);
                }
                else
                {
                    session.ComboEndTime = session.ComboEndTime.Value.AddMinutes(comboMins);
                }

                session.EndTime = session.ComboEndTime;
                session.DurationHours = (int)Math.Ceiling(session.ComboDurationMinutes / 60.0);
                session.DurationMinutes = session.ComboDurationMinutes;
                session.RemainingMinutes = Math.Max(0, (int)(session.ComboEndTime.Value - DateTime.UtcNow).TotalMinutes);

                // Table fee calculation: 0 if now <= ComboEndTime, or overtime fee if now > ComboEndTime
                var now = DateTime.UtcNow;
                if (now > session.ComboEndTime.Value)
                {
                    var overSeconds = (now - session.ComboEndTime.Value).TotalSeconds;
                    session.TotalPrice = Math.Round((decimal)overSeconds / 3600m * (table?.HourlyRate ?? 0));
                }
                else
                {
                    session.TotalPrice = 0;
                }

                _unitOfWork.Repository<TableSession>().Update(session);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApplyComboResultDto
                {
                    Success = true,
                    Message = $"Áp dụng / Gia hạn combo '{comboDto.Name}' cho bàn thành công!",
                    Combo = comboDto,
                    OrderId = orderId
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = $"Lỗi khi áp dụng combo: {ex.Message}"
                };
            }
        }
    }
}
