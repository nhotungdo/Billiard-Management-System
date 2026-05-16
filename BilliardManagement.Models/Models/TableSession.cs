using BilliardManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class TableSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TableId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        public int? DurationMinutes { get; set; }

        public decimal? TotalPrice { get; set; }

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TableId")]
        public virtual BilliardTable? BilliardTable { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
