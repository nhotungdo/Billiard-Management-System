using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
