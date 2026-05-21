using BilliardManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class BilliardTable
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TableType { get; set; } = string.Empty;

        [Required]
        [Column("PricePerHour")]
        public decimal HourlyRate { get; set; }

        [Required]
        public TableStatus Status { get; set; } = TableStatus.Available;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
    }
}
