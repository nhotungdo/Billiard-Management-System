using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập.")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên cơ sở / Câu lạc bộ.")]
        [StringLength(150, ErrorMessage = "Tên cơ sở không quá 150 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        public string? PhoneNumber { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = default!;
    }

    public class ResetPasswordForUserDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }

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
        [Required(ErrorMessage = "Tên bàn không được để trống.")]
        [StringLength(50, ErrorMessage = "Tên bàn tối đa 50 ký tự.")]
        public string TableName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại bàn.")]
        public string TableType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá mỗi giờ.")]
        [Range(0, 10000000, ErrorMessage = "Giá mỗi giờ từ 0 đ đến 10.000.000 đ.")]
        public decimal PricePerHour { get; set; }

        public string Status { get; set; } = "Available";

        [StringLength(250, ErrorMessage = "Mô tả tối đa 250 ký tự.")]
        public string? Description { get; set; }
    }

    public class UpdateTableStatusRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public string Status { get; set; } = string.Empty;
    }

    public class TableStatusUpdateDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool Force { get; set; } = false;
    }

    public class TableStatusHistoryDto
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int OldStatus { get; set; }
        public string OldStatusName { get; set; } = string.Empty;
        public int NewStatus { get; set; }
        public string NewStatusName { get; set; } = string.Empty;
        public Guid? ChangedById { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }

    public class StartSessionRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn bàn.")]
        public Guid TableId { get; set; }

        [Range(1, 24, ErrorMessage = "Thời gian từ 1 đến 24 giờ.")]
        public int DurationHours { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng.")]
        [StringLength(100, ErrorMessage = "Tên khách hàng tối đa 100 ký tự.")]
        public string? CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại khách hàng.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        public string? CustomerPhone { get; set; }

        [Range(0, 2, ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        public int PaymentMethod { get; set; } = 0;
    }

    public class ExtendSessionRequest
    {
        [Range(1, 1440, ErrorMessage = "Thời gian gia hạn từ 1 đến 1440 phút.")]
        public int AdditionalMinutes { get; set; }
    }

    public class EndSessionRequest
    {
        public int PaymentMethod { get; set; }
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
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
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
        public Guid CategoryId { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Range(0, 100000000, ErrorMessage = "Giá sản phẩm phải từ 0 đ.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        public Guid CategoryId { get; set; }

        [Range(0, 100000, ErrorMessage = "Tồn kho từ 0 đến 100.000.")]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public DateTime OrderTime { get; set; }
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Session ID không được để trống.")]
        public Guid SessionId { get; set; }
        public Guid TableSessionId { get => SessionId; set => SessionId = value; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng order từ 1 đến 1000.")]
        public int Quantity { get; set; }
    }

    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public int PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string TableType { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public decimal PlayingFee { get; set; }
        public decimal ServiceFee { get; set; }
    }

    public class CreateInvoiceDto
    {
        public int PaymentMethod { get; set; }
    }

    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [JsonConverter(typeof(RoleJsonConverter))]
        public int Role { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class UserDeletionResultDto
    {
        public int CustomersServed { get; set; }
        public int ItemsSold { get; set; }
    }

    public class RevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }

    public class DashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public int ActiveTables { get; set; }
        public int OrdersToday { get; set; }
        public string TopCustomer { get; set; } = string.Empty;

        [JsonPropertyName("todayRevenue")]
        public decimal TodayRevenue { get => TotalRevenue; set => TotalRevenue = value; }

        [JsonPropertyName("totalOrdersToday")]
        public int TotalOrdersToday { get => OrdersToday; set => OrdersToday = value; }
    }

    public class ShiftDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(50, ErrorMessage = "Tên danh mục tối đa 50 ký tự.")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Mô tả tối đa 250 ký tự.")]
        public string? Description { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class DailyRevenueDto { public DateTime Date { get; set; } public decimal TotalRevenue { get; set; } }

    public class RevenueDetailDto
    {
        public Guid InvoiceId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public decimal Subtotal { get; set; }
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
        public List<RevenueDetailDto> Revenues { get; set; } = new();
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

    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalVisits { get; set; }
        public decimal TotalPlayHours { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? FirstVisitDate { get; set; }
        public DateTime? LastVisitDate { get; set; }
    }

    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên khách hàng.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class CustomerUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên khách hàng.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số và bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class CustomerTopSpenderDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int TotalVisits { get; set; }
    }

    public class CustomerDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveCustomersThisMonth { get; set; }
        public decimal AverageRevenuePerCustomer { get; set; }
    }
}
