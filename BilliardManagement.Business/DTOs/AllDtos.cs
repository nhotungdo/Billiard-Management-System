using System;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Business.DTOs
{
    public class UserDto { public Guid Id { get; set; } public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string? PhoneNumber { get; set; } public UserRole Role { get; set; } public bool IsActive { get; set; } }
    public class LoginDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterDto { public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? PhoneNumber { get; set; } }
    public class AuthResponseDto { public string Token { get; set; } = string.Empty; public UserDto User { get; set; } = default!; }

    public class TableDto { public Guid Id { get; set; } public string TableName { get; set; } = string.Empty; public TableType Type { get; set; } public TableStatus Status { get; set; } public decimal HourlyRate { get; set; } }
    public class CreateTableDto { public string TableName { get; set; } = string.Empty; public TableType Type { get; set; } public decimal HourlyRate { get; set; } }
    
    public class SessionDto { public Guid Id { get; set; } public Guid TableId { get; set; } public Guid UserId { get; set; } public DateTime StartTime { get; set; } public DateTime? EndTime { get; set; } public int? DurationMinutes { get; set; } public decimal? TotalPrice { get; set; } public SessionStatus Status { get; set; } }

    public class ProductDto { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public decimal Price { get; set; } public string Category { get; set; } = string.Empty; public int Stock { get; set; } public string? ImageUrl { get; set; } }
    public class CreateProductDto { public string Name { get; set; } = string.Empty; public decimal Price { get; set; } public string Category { get; set; } = string.Empty; public int Stock { get; set; } public string? ImageUrl { get; set; } }

    public class OrderDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public Guid UserId { get; set; } public decimal TotalAmount { get; set; } public OrderStatus Status { get; set; } }
    public class OrderItemDto { public Guid Id { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
    public class CreateOrderDto { public Guid SessionId { get; set; } public List<CreateOrderItemDto> Items { get; set; } = new(); }
    public class CreateOrderItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } }

    public class BillDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public decimal Subtotal { get; set; } public decimal Discount { get; set; } public decimal Total { get; set; } public PaymentMethod PaymentMethod { get; set; } public bool IsPaid { get; set; } }
    public class GenerateBillDto { public decimal Discount { get; set; } public PaymentMethod PaymentMethod { get; set; } }
    
    public class ShiftDto { public Guid Id { get; set; } public Guid UserId { get; set; } public DateTime CheckIn { get; set; } public DateTime? CheckOut { get; set; } public decimal Revenue { get; set; } }
    
    public class DailyRevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }
}
