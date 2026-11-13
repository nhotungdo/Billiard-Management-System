using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BilliardManagement.Models.Tenant;

namespace BilliardManagement.Models.Models
{
    public class Shift : ITenantEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StoreId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime ShiftDate { get; set; }

        [Required]
        [Column("StartShift")]
        public DateTime StartTime { get; set; }

        [Column("EndShift")]
        public DateTime? EndTime { get; set; }

        public decimal TotalRevenue { get; set; } = 0;

        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
