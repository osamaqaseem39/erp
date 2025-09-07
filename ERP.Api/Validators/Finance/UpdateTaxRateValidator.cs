using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateTaxRateValidator : BaseValidator<UpdateTaxRateDto>
{
    public UpdateTaxRateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Tax rate ID is required");

        RuleFor(x => x.TaxCode)
            .NotEmpty().WithMessage("Tax code is required")
            .MaximumLength(20).WithMessage("Tax code cannot exceed 20 characters");

        RuleFor(x => x.TaxName)
            .NotEmpty().WithMessage("Tax name is required")
            .MaximumLength(100).WithMessage("Tax name cannot exceed 100 characters");

        RuleFor(x => x.TaxType)
            .NotEmpty().WithMessage("Tax type is required")
            .MaximumLength(50).WithMessage("Tax type cannot exceed 50 characters");

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Tax rate must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Tax rate cannot exceed 100%");

        RuleFor(x => x.EffectiveFrom)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Effective from date cannot be in the future")
            .When(x => x.EffectiveFrom.HasValue);

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom).WithMessage("Effective to date must be after effective from date")
            .When(x => x.EffectiveTo.HasValue && x.EffectiveFrom.HasValue);
    }
}