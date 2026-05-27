using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;
using BilliardManagement.Common.Responses;

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
        Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto);
    }

    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTablesAsync();
        Task<PagedResult<TableDto>> GetPagedTablesAsync(TableQueryParameters query);
        Task<TableDto> GetTableByIdAsync(Guid id);
        Task<TableDto> CreateTableAsync(CreateTableDto dto, Guid? createdBy = null);
        Task<TableDto> UpdateTableStatusAsync(Guid id, TableStatus status, Guid? updatedBy = null, string? reason = null, bool force = false, bool isAdmin = false);
        Task<TableDto> UpdateTableAsync(Guid id, CreateTableDto dto, Guid? updatedBy = null);
        Task<bool> DeleteTableAsync(Guid id);
        Task<IEnumerable<TableStatusHistoryDto>> GetTableHistoryAsync(Guid id);
    }

    public interface ISessionService
    {
        Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId, int durationHours);
        Task<SessionDto> ExtendSessionAsync(Guid sessionId, int additionalMinutes, Guid? staffUserId = null);
        Task<SessionDto> EndSessionAsync(Guid sessionId, GenerateBillDto? billDto = null, Guid? staffUserId = null);
        Task<IEnumerable<SessionDto>> GetActiveSessionsAsync();
        Task<PagedResult<SessionDto>> GetPagedSessionsAsync(SessionQueryParameters query);
        Task<IEnumerable<TableDashboardDto>> GetTableDashboardAsync();
    }

    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<PagedResult<ProductDto>> GetPagedProductsAsync(ProductQueryParameters query);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid? createdBy = null);
        Task<ProductDto> UpdateProductAsync(Guid id, CreateProductDto dto, Guid? updatedBy = null);
        Task<bool> DeleteProductAsync(Guid id);
        Task<bool> CheckDuplicateNameAsync(string name, Guid? excludeId = null);
    }

    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid userId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<PagedResult<OrderDto>> GetPagedOrdersAsync(OrderQueryParameters query);
        Task<OrderDto> GetOrderByIdAsync(Guid id);
        Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status);
    }

    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(Guid id, CreateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}
