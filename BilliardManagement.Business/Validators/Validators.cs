using FluentValidation;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Models.Enums;

namespace BilliardManagement.Business.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }

    public class CreateTableDtoValidator : AbstractValidator<CreateTableDto>
    {
        private static readonly string[] AllowedTypes = { "Pool 8 Ball", "Pool 9 Ball", "Snooker", "VIP" };
        private static readonly string[] AllowedStatuses = { "Available", "Reserved", "Maintenance" };

        public CreateTableDtoValidator()
        {
            RuleFor(x => x.TableName).NotEmpty().WithMessage("Tên bàn không được để trống");
            RuleFor(x => x.PricePerHour).GreaterThan(0).WithMessage("Giá theo giờ phải lớn hơn 0");
            RuleFor(x => x.TableType)
                .Must(t => AllowedTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Loại bàn không hợp lệ");
            RuleFor(x => x.Status)
                .Must(s => AllowedStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Không thể tạo bàn với trạng thái Đang chơi");
        }
    }

    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        private static readonly string[] AllowedCategories = { "Nước ngọt", "Cafe", "Bia", "Snack", "Trà sữa" };

        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên sản phẩm không được để trống");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Giá bán phải lớn hơn 0");
            RuleFor(x => x.Category)
                .Must(c => AllowedCategories.Contains(c, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Danh mục không hợp lệ");
        }
    }
}
