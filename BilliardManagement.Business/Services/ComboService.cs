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
                filter: s => s.Id == request.TableSessionId && !s.IsFinished
            );

            if (session == null)
            {
                return new ApplyComboResultDto
                {
                    Success = false,
                    Message = "Phiên chơi không tồn tại hoặc đã kết thúc."
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create an order for the combo
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    TableSessionId = session.Id,
                    OrderedBy = userId,
                    OrderTime = DateTime.UtcNow,
                    TotalAmount = comboDto.Price,
                    Status = OrderStatus.Completed
                };

                // Create OrderItems from combo items
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

                    // Deduct stock if available
                    if (product != null && product.StockQuantity >= item.Quantity)
                    {
                        product.StockQuantity -= item.Quantity;
                        _unitOfWork.Repository<Product>().Update(product);
                    }
                }

                await _unitOfWork.Repository<Order>().AddAsync(order);

                // If combo includes table play hours, add to session duration
                if (comboDto.PlayingHours > 0)
                {
                    session.DurationHours += comboDto.PlayingHours;
                    session.DurationMinutes = session.DurationHours * 60;
                    if (session.EndTime.HasValue)
                    {
                        session.EndTime = session.EndTime.Value.AddHours(comboDto.PlayingHours);
                        session.RemainingMinutes = Math.Max(0, (int)(session.EndTime.Value - DateTime.UtcNow).TotalMinutes);
                    }
                    _unitOfWork.Repository<TableSession>().Update(session);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ApplyComboResultDto
                {
                    Success = true,
                    Message = $"Áp dụng combo '{comboDto.Name}' cho bàn thành công!",
                    Combo = comboDto,
                    OrderId = order.Id
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
