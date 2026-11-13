using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Data;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;

namespace BilliardManagement.Business.Services
{
    public class StoreService : IStoreService
    {
        private readonly BilliardManagementDbContext _context;

        public StoreService(BilliardManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoreDto>> GetAllAsync()
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            var result = new List<StoreDto>();
            foreach (var store in stores)
            {
                result.Add(await MapAsync(store));
            }
            return result;
        }

        public async Task<StoreDto> GetByIdAsync(Guid id)
        {
            var store = await _context.Stores.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) throw new CustomException("Không tìm thấy cửa hàng", 404);
            return await MapAsync(store);
        }

        public async Task<StoreDto> CreateAsync(CreateStoreDto dto)
        {
            Validate(dto.Name, dto.Email);

            var store = new Store
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Address = dto.Address?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Email = dto.Email?.Trim(),
                LogoUrl = dto.LogoUrl?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return await MapAsync(store);
        }

        public async Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto dto)
        {
            Validate(dto.Name, dto.Email);

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) throw new CustomException("Không tìm thấy cửa hàng", 404);

            store.Name = dto.Name.Trim();
            store.Address = dto.Address?.Trim();
            store.PhoneNumber = dto.PhoneNumber?.Trim();
            store.Email = dto.Email?.Trim();
            store.LogoUrl = dto.LogoUrl?.Trim();
            store.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return await MapAsync(store);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) return false;

            var hasActiveSessions = await _context.TableSessions
                .IgnoreQueryFilters()
                .AnyAsync(s => s.StoreId == id && s.Status == SessionStatus.Active && !s.IsFinished);

            if (hasActiveSessions)
                throw new CustomException("Không thể vô hiệu hóa cửa hàng đang có phiên chơi hoạt động", 400);

            store.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<StoreDto> MapAsync(Store store)
        {
            var userCount = await _context.Users
                .IgnoreQueryFilters()
                .CountAsync(u => u.StoreId == store.Id);

            var tableCount = await _context.BilliardTables
                .IgnoreQueryFilters()
                .CountAsync(t => t.StoreId == store.Id);

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                PhoneNumber = store.PhoneNumber,
                Email = store.Email,
                LogoUrl = store.LogoUrl,
                IsActive = store.IsActive,
                CreatedAt = store.CreatedAt,
                UserCount = userCount,
                TableCount = tableCount
            };
        }

        private static void Validate(string name, string? email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CustomException("Tên cửa hàng không được để trống", 400);

            if (!string.IsNullOrWhiteSpace(email) &&
                !System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new CustomException("Email không hợp lệ", 400);
            }
        }
    }
}
