using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateTaxCodeValidator : BaseValidator<UpdateTaxCodeDto>
{
    public UpdateTaxCodeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.TaxCode)
            .MaximumLength(50)
            .When(x => x.TaxCode != null);

        RuleFor(x => x.TaxName)
            .MaximumLength(200)
            .When(x => x.TaxName != null);

        RuleFor(x => x.TaxType)
            .MaximumLength(50)
            .When(x => x.TaxType != null);

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
