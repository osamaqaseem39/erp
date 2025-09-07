using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateJournalEntryValidator : BaseValidator<CreateJournalEntryDto>
{
    public CreateJournalEntryValidator()
    {
        RuleFor(x => x.JournalId)
            .NotEmpty().WithMessage("Journal ID is required");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required")
            .MaximumLength(20).WithMessage("Account code cannot exceed 20 characters");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(200).WithMessage("Account name cannot exceed 200 characters");

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("Debit amount must be greater than or equal to 0");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit amount must be greater than or equal to 0");

        RuleFor(x => x)
            .Must(x => x.Debit > 0 || x.Credit > 0)
            .WithMessage("Either debit or credit amount must be greater than 0");

        RuleFor(x => x)
            .Must(x => !(x.Debit > 0 && x.Credit > 0))
            .WithMessage("Cannot have both debit and credit amounts greater than 0");

        RuleFor(x => x.Narration)
            .MaximumLength(500).WithMessage("Narration cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Narration));

        RuleFor(x => x.EntityReferenceTable)
            .MaximumLength(100).WithMessage("Entity reference table cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.EntityReferenceTable));
    }
}