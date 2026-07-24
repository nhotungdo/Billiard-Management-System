using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;

namespace BilliardManagement.Business.Interfaces
{
    public interface IComboService
    {
        Task<IEnumerable<ComboDto>> GetAllCombosAsync(string? search = null, bool? activeOnly = null);
        Task<ComboDto?> GetComboByIdAsync(Guid id);
        Task<ComboDto?> GetComboByCodeAsync(string comboCode);
        Task<ComboDto> CreateComboAsync(CreateComboDto dto);
        Task<ComboDto?> UpdateComboAsync(Guid id, UpdateComboDto dto);
        Task<bool> ToggleComboStatusAsync(Guid id);
        Task<bool> DeleteComboAsync(Guid id);
        Task<ApplyComboResultDto> ApplyComboToSessionAsync(ApplyComboRequestDto request, Guid userId);
    }
}
