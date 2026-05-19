using FluentValidation;
using BilliardManagement.Business.DTOs;

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
        public CreateTableDtoValidator()
        {
            RuleFor(x => x.TableName).NotEmpty().WithMessage("Table Name is required");
            RuleFor(x => x.HourlyRate).GreaterThan(0).WithMessage("Hourly Rate must be > 0");
        }
    }
}
