using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class SessionCombo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TableSessionId { get; set; }

        public Guid? ComboId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ComboName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TableSessionId")]
        public virtual TableSession? TableSession { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }
    }
}
