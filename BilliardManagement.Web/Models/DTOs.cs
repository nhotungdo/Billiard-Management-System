using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BilliardManagement.Web.Json;

namespace BilliardManagement.Web.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [JsonConverter(typeof(RoleJsonConverter))]
        public int Role { get; set; }
        public bool IsActive { get; set; }
    }
    public class LoginRequest { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterRequest { public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? PhoneNumber { get; set; } }
    public class LoginResponse { public string Token { get; set; } = string.Empty; public UserDto User { get; set; } = default!; }

    public class TableDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal PricePerHour { get; set; }
        public string? Description { get; set; }
    }

    public class CreateTableDto
    {
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public string Status { get; set; } = "Available";
        public string? Description { get; set; }
    }
    public class UpdateTableStatusRequest { public string Status { get; set; } = string.Empty; }

    public class StartSessionRequest { public Guid TableId { get; set; } public int DurationHours { get; set; } = 1; }
    public class ExtendSessionRequest { public int AdditionalMinutes { get; set; } }
    public class EndSessionRequest { public decimal Discount { get; set; } public int PaymentMethod { get; set; } }

    public class SessionOrderLineDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class SessionDto
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public Guid UserId { get; set; }
        public string? TableName { get; set; }
        public string? TableType { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationHours { get; set; }
        public int? DurationMinutes { get; set; }
        public int RemainingMinutes { get; set; }
        public int RemainingSeconds { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OrdersTotal { get; set; }
        public decimal CurrentTotal { get; set; }
        public bool IsFinished { get; set; }
        public bool IsExpired { get; set; }
        public int Status { get; set; }
        public List<SessionOrderLineDto> OrderLines { get; set; } = new();
    }

    public class TableDashboardDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal HourlyRate { get; set; }
        public SessionDto? ActiveSession { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class OrderDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public Guid UserId { get; set; } public decimal TotalAmount { get; set; } public int Status { get; set; } }
    public class OrderItemDto { public Guid Id { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
    public class CreateOrderDto
    {
        public Guid SessionId { get; set; }
        public Guid TableSessionId { get => SessionId; set => SessionId = value; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
    public class CreateOrderItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } }

    public class InvoiceDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public decimal Subtotal { get; set; } public decimal Discount { get; set; } public decimal Total { get; set; } public int PaymentMethod { get; set; } public bool IsPaid { get; set; } }
    public class CreateInvoiceDto { public decimal Discount { get; set; } public int PaymentMethod { get; set; } }

    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [JsonConverter(typeof(RoleJsonConverter))]
        public int Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class RevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }

    public class DashboardDto { public decimal TotalRevenue { get; set; } public int ActiveTables { get; set; } public int OrdersToday { get; set; } public string TopCustomer { get; set; } = string.Empty; }
}
