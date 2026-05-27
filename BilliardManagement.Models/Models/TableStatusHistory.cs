using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Models.Models
{
    public class TableStatusHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TableId { get; set; }

        [Required]
        public TableStatus OldStatus { get; set; }

        [Required]
        public TableStatus NewStatus { get; set; }

        public Guid? ChangedById { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Reason { get; set; }

        [ForeignKey("TableId")]
        public virtual BilliardTable? BilliardTable { get; set; }

        [ForeignKey("ChangedById")]
        public virtual User? ChangedByUser { get; set; }
    }
}
