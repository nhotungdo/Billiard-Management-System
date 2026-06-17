using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public int TotalVisits { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPlayHours { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0;

        public DateTime? FirstVisitDate { get; set; }
        
        public DateTime? LastVisitDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
