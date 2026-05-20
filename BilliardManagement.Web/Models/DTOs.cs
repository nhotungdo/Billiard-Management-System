using System;
using System.Collections.Generic;

namespace BilliardManagement.Web.Models
{
    public class UserDto { public Guid Id { get; set; } public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string? PhoneNumber { get; set; } public int Role { get; set; } public bool IsActive { get; set; } }
    public class LoginRequest { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterRequest { public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? PhoneNumber { get; set; } }
    public class LoginResponse { public string Token { get; set; } = string.Empty; public UserDto User { get; set; } = default!; }

    public class TableDto { public Guid Id { get; set; } public string TableName { get; set; } = string.Empty; public int Type { get; set; } public int Status { get; set; } public decimal HourlyRate { get; set; } }
    public class CreateTableDto { public string TableName { get; set; } = string.Empty; public int Type { get; set; } public decimal HourlyRate { get; set; } }
    
    public class SessionDto { public Guid Id { get; set; } public Guid TableId { get; set; } public Guid UserId { get; set; } public DateTime StartTime { get; set; } public DateTime? EndTime { get; set; } public int? DurationMinutes { get; set; } public decimal? TotalPrice { get; set; } public int Status { get; set; } }

    public class ProductDto { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public decimal Price { get; set; } public string Category { get; set; } = string.Empty; public int Stock { get; set; } public string? ImageUrl { get; set; } }
    public class CreateProductDto { public string Name { get; set; } = string.Empty; public decimal Price { get; set; } public string Category { get; set; } = string.Empty; public int Stock { get; set; } public string? ImageUrl { get; set; } }

    public class OrderDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public Guid UserId { get; set; } public decimal TotalAmount { get; set; } public int Status { get; set; } }
    public class OrderItemDto { public Guid Id { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
    public class CreateOrderDto { public Guid SessionId { get; set; } public List<CreateOrderItemDto> Items { get; set; } = new(); }
    public class CreateOrderItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } }

    public class InvoiceDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public decimal Subtotal { get; set; } public decimal Discount { get; set; } public decimal Total { get; set; } public int PaymentMethod { get; set; } public bool IsPaid { get; set; } }
    public class CreateInvoiceDto { public decimal Discount { get; set; } public int PaymentMethod { get; set; } }
    
    public class StaffDto { public Guid Id { get; set; } public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string? PhoneNumber { get; set; } public int Role { get; set; } public bool IsActive { get; set; } }
    
    public class RevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }
    
    public class DashboardDto { public decimal TotalRevenue { get; set; } public int ActiveTables { get; set; } public int OrdersToday { get; set; } public string TopCustomer { get; set; } = string.Empty; }
}
