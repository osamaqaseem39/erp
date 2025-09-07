using ERP.Shared.DTOs.Admin;
using FluentValidation;

namespace ERP.Api.Validators.Admin;

public class UpdateSystemSettingValidator : AbstractValidator<UpdateSystemSettingDto>
{
    public UpdateSystemSettingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.SettingValue)
            .NotEmpty().WithMessage("Setting value is required")
            .MaximumLength(500).WithMessage("Setting value cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");

        RuleFor(x => x.SettingType)
            .MaximumLength(50).WithMessage("Setting type cannot exceed 50 characters");
    }
}