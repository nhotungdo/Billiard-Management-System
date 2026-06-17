using System;

namespace BilliardManagement.Business.DTOs
{
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
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class CustomerUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class CustomerTopSpenderDto
    {
        public int Rank { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalVisits { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CustomerDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersLast30Days { get; set; }
        public decimal RetentionRate { get; set; }
    }
}
