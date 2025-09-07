using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateJournalValidator : BaseValidator<CreateJournalDto>
{
    public CreateJournalValidator()
    {
        RuleFor(x => x.JournalType)
            .NotEmpty().WithMessage("Journal type is required")
            .MaximumLength(50).WithMessage("Journal type cannot exceed 50 characters");

        RuleFor(x => x.JournalDate)
            .NotEmpty().WithMessage("Journal date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Journal date cannot be in the future");

        RuleFor(x => x.ReferenceNo)
            .MaximumLength(100).WithMessage("Reference number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNo));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0")
            .When(x => x.ExchangeRate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(20).WithMessage("Status cannot exceed 20 characters");
    }
}