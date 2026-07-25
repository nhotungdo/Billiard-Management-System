using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BilliardManagement.Business.DTOs
{
    public class ComboItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class ComboDto
    {
        public Guid Id { get; set; }
        public string ComboCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int PlayingHours { get; set; }
        public bool IsVip { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ComboItemDto> Items { get; set; } = new();
    }

    public class CreateComboItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }

    public class CreateComboDto
    {
        [Required]
        [MaxLength(50)]
        public string ComboCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000000)]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, 24)]
        public int PlayingHours { get; set; } = 0;

        public bool IsVip { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public List<CreateComboItemDto> Items { get; set; } = new();
    }

    public class UpdateComboDto
    {
        [Required]
        [MaxLength(50)]
        public string ComboCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000000)]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, 24)]
        public int PlayingHours { get; set; } = 0;

        public bool IsVip { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public List<CreateComboItemDto> Items { get; set; } = new();
    }

    public class ApplyComboRequestDto
    {
        [Required]
        public Guid TableSessionId { get; set; }

        [Required]
        public string ComboCode { get; set; } = string.Empty;
    }

    public class ApplyComboResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ComboDto? Combo { get; set; }
        public Guid? OrderId { get; set; }
    }

    public class SessionComboDto
    {
        public Guid Id { get; set; }
        public Guid TableSessionId { get; set; }
        public Guid? ComboId { get; set; }
        public string ComboName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
