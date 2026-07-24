using BilliardManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TableSessionId { get; set; }

        [Required]
        public Guid OrderedBy { get; set; }

        public DateTime OrderTime { get; set; } = DateTime.UtcNow;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public bool IsComboOrder { get; set; } = false;

        // Navigation properties
        [ForeignKey("TableSessionId")]
        public virtual TableSession? TableSession { get; set; }

        [ForeignKey("OrderedBy")]
        public virtual User? User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
