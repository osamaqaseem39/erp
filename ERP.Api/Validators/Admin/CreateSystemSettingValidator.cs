using ERP.Shared.DTOs.Admin;
using FluentValidation;

namespace ERP.Api.Validators.Admin;

public class CreateSystemSettingValidator : AbstractValidator<CreateSystemSettingDto>
{
    public CreateSystemSettingValidator()
    {
        RuleFor(x => x.SettingKey)
            .NotEmpty().WithMessage("Setting key is required")
            .MaximumLength(100).WithMessage("Setting key cannot exceed 100 characters");

        RuleFor(x => x.SettingValue)
            .NotEmpty().WithMessage("Setting value is required")
            .MaximumLength(500).WithMessage("Setting value cannot exceed 500 characters");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");
    }
}