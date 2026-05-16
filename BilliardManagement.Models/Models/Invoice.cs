using BilliardManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TableSessionId { get; set; }

        public Guid? OrderId { get; set; }

        [Required]
        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public decimal FinalAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TableSessionId")]
        public virtual TableSession? TableSession { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
