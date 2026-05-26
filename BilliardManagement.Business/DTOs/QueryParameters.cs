using System;

namespace BilliardManagement.Business.DTOs
{
    public class BaseQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
    }

    public class InvoiceQueryParameters : BaseQueryParameters
    {
        public bool? IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
    }

    public class OrderQueryParameters : BaseQueryParameters
    {
        public int? Status { get; set; }
        public Guid? SessionId { get; set; }
    }

    public class SessionQueryParameters : BaseQueryParameters
    {
        public int? Status { get; set; }
        public Guid? TableId { get; set; }
        public bool? IsFinished { get; set; }
    }

    public class TableQueryParameters : BaseQueryParameters
    {
        public int? Status { get; set; }
        public string? TableType { get; set; }
    }

    public class ProductQueryParameters : BaseQueryParameters
    {
        public Guid? CategoryId { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
