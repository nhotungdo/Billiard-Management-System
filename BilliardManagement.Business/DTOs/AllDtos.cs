using System;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Business.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UserDeletionResultDto
    {
        public int CustomersServed { get; set; }
        public int ItemsSold { get; set; }
    }

    public class LoginDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterDto { public string FullName { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? PhoneNumber { get; set; } }
    public class AuthResponseDto { public string Token { get; set; } = string.Empty; public UserDto User { get; set; } = default!; }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class TableDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public TableStatus Status { get; set; }
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

    public class TableCreatedDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal PricePerHour { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateTableStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class TableStatusChangedDto
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
    
    public class StartSessionRequest
    {
        public Guid TableId { get; set; }
        public int DurationHours { get; set; } = 1;
    }

    public class ExtendSessionRequest
    {
        public int AdditionalMinutes { get; set; }
    }

    public class EndSessionRequest
    {
        public decimal Discount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

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
        public SessionStatus Status { get; set; }
        public List<SessionOrderLineDto> OrderLines { get; set; } = new();
    }

    public class TableDashboardDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public TableStatus Status { get; set; }
        public decimal HourlyRate { get; set; }
        public SessionDto? ActiveSession { get; set; }
    }

    public class SessionRealtimeDto
    {
        public Guid SessionId { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int Status { get; set; }
        public int TableStatus { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationHours { get; set; }
        public int RemainingMinutes { get; set; }
        public int RemainingSeconds { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OrdersTotal { get; set; }
        public decimal CurrentTotal { get; set; }
        public bool IsExpired { get; set; }
        public bool IsFinished { get; set; }
        public string TimerLevel { get; set; } = "ok";
        public List<SessionOrderLineDto> OrderLines { get; set; } = new();
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class ProductCreatedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class OrderDto { public Guid Id { get; set; } public Guid SessionId { get; set; } public Guid UserId { get; set; } public decimal TotalAmount { get; set; } public OrderStatus Status { get; set; } }
    public class OrderItemDto { public Guid Id { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
    public class CreateOrderDto
    {
        public Guid SessionId { get; set; }
        public Guid TableSessionId { get => SessionId; set => SessionId = value; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
    public class CreateOrderItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } }

    public class BillDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public decimal PlayingFee { get; set; }
        public decimal ServiceFee { get; set; }
    }
    public class GenerateBillDto { public decimal Discount { get; set; } public PaymentMethod PaymentMethod { get; set; } }
    
    public class ShiftDto { public Guid Id { get; set; } public Guid UserId { get; set; } public DateTime CheckIn { get; set; } public DateTime? CheckOut { get; set; } public decimal Revenue { get; set; } }
    
    public class DailyRevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }

    public class RevenueDto
    {
        public Guid InvoiceId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double PlayTimeMinutes { get; set; }
        public decimal PlayingFee { get; set; }
        public decimal ServiceFee { get; set; }
    }

    public class PersonalRevenueDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int InvoicesCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal UnfilteredTotalRevenue { get; set; }
        public int UnfilteredInvoicesCount { get; set; }
        public List<RevenueDto> Revenues { get; set; } = new();
    }

    public class StaffRevenueDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int InvoicesCount { get; set; }
    }

    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TodayInvoices { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<StaffRevenueDto> StaffRevenues { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
    }

    public class RevenueFilterQuery
    {
        public Guid? StaffId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
