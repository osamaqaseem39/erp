using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateJournalEntryValidator : BaseValidator<UpdateJournalEntryDto>
{
    public UpdateJournalEntryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Journal entry ID is required");

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required")
            .MaximumLength(20).WithMessage("Account code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountCode));

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(200).WithMessage("Account name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountName));

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Debit amount must be greater than or equal to 0")
            .When(x => x.Debit.HasValue);

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit amount must be greater than or equal to 0")
            .When(x => x.Credit.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.Debit > 0 && x.Credit > 0))
            .WithMessage("Cannot have both debit and credit amounts greater than 0")
            .When(x => x.Debit.HasValue && x.Credit.HasValue);

        RuleFor(x => x.Narration)
            .MaximumLength(500).WithMessage("Narration cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Narration));

        RuleFor(x => x.EntityReferenceTable)
            .MaximumLength(100).WithMessage("Entity reference table cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.EntityReferenceTable));
    }
}