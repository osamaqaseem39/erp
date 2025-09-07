using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateCurrencyValidator : BaseValidator<CreateCurrencyDto>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .MaximumLength(10).WithMessage("Currency code cannot exceed 10 characters");

        RuleFor(x => x.CurrencyName)
            .NotEmpty().WithMessage("Currency name is required")
            .MaximumLength(100).WithMessage("Currency name cannot exceed 100 characters");

        RuleFor(x => x.Iso4217)
            .MaximumLength(3).WithMessage("ISO 4217 code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Iso4217));

        RuleFor(x => x.Symbol)
            .MaximumLength(10).WithMessage("Symbol cannot exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.Symbol));
    }
}