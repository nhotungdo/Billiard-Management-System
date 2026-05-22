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
        Task<UserDeletionResultDto> DeleteUserAsync(Guid id);
    }

    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTablesAsync();
        Task<TableDto> GetTableByIdAsync(Guid id);
        Task<TableDto> CreateTableAsync(CreateTableDto dto, Guid? createdBy = null);
        Task<TableDto> UpdateTableStatusAsync(Guid id, TableStatus status, Guid? updatedBy = null);
        Task<TableDto> UpdateTableAsync(Guid id, CreateTableDto dto, Guid? updatedBy = null);
        Task<bool> DeleteTableAsync(Guid id);
    }

    public interface ISessionService
    {
        Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId, int durationHours);
        Task<SessionDto> ExtendSessionAsync(Guid sessionId, int additionalMinutes, Guid? staffUserId = null);
        Task<SessionDto> EndSessionAsync(Guid sessionId, GenerateBillDto? billDto = null, Guid? staffUserId = null);
        Task<IEnumerable<SessionDto>> GetActiveSessionsAsync();
        Task<IEnumerable<TableDashboardDto>> GetTableDashboardAsync();
    }

    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null);
        Task<ProductDto> UpdateProductAsync(Guid id, CreateProductDto dto, Guid? updatedBy = null);
        Task<bool> DeleteProductAsync(Guid id);
        Task<bool> CheckDuplicateNameAsync(string name, Guid? excludeId = null);
    }

    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid userId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderByIdAsync(Guid id);
    }
}
