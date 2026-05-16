using BilliardManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

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
        public decimal PricePerHour { get; set; }

        [Required]
        public TableStatus Status { get; set; } = TableStatus.Available;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
    }
}
