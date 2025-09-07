using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateExchangeRateValidator : BaseValidator<CreateExchangeRateDto>
{
    public CreateExchangeRateValidator()
    {
        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("From currency is required")
            .MaximumLength(10).WithMessage("From currency cannot exceed 10 characters");

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("To currency is required")
            .MaximumLength(10).WithMessage("To currency cannot exceed 10 characters")
            .NotEqual(x => x.FromCurrency).WithMessage("From currency and To currency cannot be the same");

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0")
            .LessThan(1000000).WithMessage("Exchange rate must be less than 1,000,000");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("Effective date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Effective date cannot be in the future");
    }
}