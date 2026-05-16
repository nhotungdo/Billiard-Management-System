using System.ComponentModel.DataAnnotations;

namespace BilliardManagement.Models.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
