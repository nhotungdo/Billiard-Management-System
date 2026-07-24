using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BilliardManagement.Web.Models
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
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ComboItemDto> Items { get; set; } = new();
    }

    public class CreateComboItemDto
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; } = 1;
    }

    public class CreateComboDto
    {
        [Required(ErrorMessage = "Mã combo là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã combo tối đa 50 ký tự")]
        public string ComboCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên combo là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên combo tối đa 150 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá combo là bắt buộc")]
        [Range(0, 100000000, ErrorMessage = "Giá combo không hợp lệ")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        [Range(0, 24, ErrorMessage = "Số giờ chơi từ 0 đến 24 giờ")]
        public int PlayingHours { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public List<CreateComboItemDto> Items { get; set; } = new();
    }

    public class UpdateComboDto : CreateComboDto
    {
        public Guid Id { get; set; }
    }

    public class ApplyComboRequestDto
    {
        [Required]
        public Guid TableSessionId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hoặc chọn mã Combo")]
        public string ComboCode { get; set; } = string.Empty;
    }

    public class ApplyComboResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ComboDto? Combo { get; set; }
        public Guid? OrderId { get; set; }
    }
}
