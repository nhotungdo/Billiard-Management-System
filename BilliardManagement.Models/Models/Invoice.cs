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
        [Column("SubTotal")]
        public decimal Subtotal { get; set; }

        [Column("DiscountAmount")]
        public decimal Discount { get; set; } = 0;

        [Required]
        [Column("FinalAmount")]
        public decimal TotalAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Column("PaidAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public bool IsPaid { get; set; } = true;

        // Navigation properties
        [ForeignKey("TableSessionId")]
        public virtual TableSession? TableSession { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        public Guid? CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
