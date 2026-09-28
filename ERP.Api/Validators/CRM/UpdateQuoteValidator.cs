using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class UpdateQuoteValidator : BaseValidator<UpdateQuoteDto>
{
    public UpdateQuoteValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Quote ID is required");

        RuleFor(x => x.QuoteNumber)
            .NotEmpty().WithMessage("Quote number is required")
            .MaximumLength(50).WithMessage("Quote number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.QuoteNumber));

        RuleFor(x => x.QuoteDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Quote date cannot be in the future")
            .When(x => x.QuoteDate.HasValue);

        RuleFor(x => x.ValidUntil)
            .GreaterThan(x => x.QuoteDate).WithMessage("Valid until date must be after quote date")
            .When(x => x.ValidUntil.HasValue && x.QuoteDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.SubTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Sub total must be greater than or equal to 0")
            .When(x => x.SubTotal.HasValue);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0")
            .When(x => x.TaxAmount.HasValue);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0")
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleForEach(x => x.QuoteLines)
            .SetValidator(new CreateQuoteLineValidator())
            .When(x => x.QuoteLines.Count > 0);
    }
}