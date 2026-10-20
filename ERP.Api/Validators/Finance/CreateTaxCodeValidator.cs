using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateTaxCodeValidator : BaseValidator<CreateTaxCodeDto>
{
    public CreateTaxCodeValidator()
    {
        RuleFor(x => x.TaxCode)
            .NotEmpty().WithMessage("Tax code is required")
            .MaximumLength(50);

        RuleFor(x => x.TaxName)
            .NotEmpty().WithMessage("Tax name is required")
            .MaximumLength(200);

        RuleFor(x => x.TaxType)
            .NotEmpty().WithMessage("Tax type is required")
            .MaximumLength(50);

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.GLAccountId)
            .NotEmpty().WithMessage("GL account is required");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
