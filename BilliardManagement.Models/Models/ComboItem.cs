using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilliardManagement.Models.Models
{
    public class ComboItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ComboId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        // Navigation properties
        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
