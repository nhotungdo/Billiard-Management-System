using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Business.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    }

    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<UserDto> UpdateUserRoleAsync(Guid id, UserRole role);
    }

    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTablesAsync();
        Task<TableDto> GetTableByIdAsync(Guid id);
        Task<TableDto> CreateTableAsync(CreateTableDto dto);
        Task<TableDto> UpdateTableStatusAsync(Guid id, TableStatus status);
    }

    public interface ISessionService
    {
        Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId);
        Task<SessionDto> EndSessionAsync(Guid sessionId);
        Task<IEnumerable<SessionDto>> GetActiveSessionsAsync();
    }

    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    }

    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid userId);
    }
}
