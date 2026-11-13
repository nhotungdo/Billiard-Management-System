using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;

namespace BilliardManagement.Business.Interfaces
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetAllAsync();
        Task<StoreDto> GetByIdAsync(Guid id);
        Task<StoreDto> CreateAsync(CreateStoreDto dto);
        Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
