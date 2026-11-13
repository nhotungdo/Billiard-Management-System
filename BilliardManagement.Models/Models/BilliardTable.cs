using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Tenant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class BilliardTable : ITenantEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoreId { get; set; }

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
