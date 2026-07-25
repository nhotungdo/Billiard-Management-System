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

        public int DurationHours { get; set; }

        public int? DurationMinutes { get; set; }

        public int RemainingMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool IsFinished { get; set; }

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TableId")]
        public virtual BilliardTable? BilliardTable { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public Guid? CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public Guid? ComboId { get; set; }
        public int ComboHours { get; set; } = 0;
        public int ComboDurationMinutes { get; set; } = 0;
        public DateTime? ComboEndTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ComboPrice { get; set; } = 0;

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }

        public virtual ICollection<SessionCombo> SessionCombos { get; set; } = new List<SessionCombo>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
