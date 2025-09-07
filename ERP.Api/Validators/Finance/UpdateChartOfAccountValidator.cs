using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateChartOfAccountValidator : BaseValidator<UpdateChartOfAccountDto>
{
    public UpdateChartOfAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required")
            .MaximumLength(20).WithMessage("Account code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountCode));

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(200).WithMessage("Account name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountName));

        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required")
            .MaximumLength(50).WithMessage("Account type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.AccountType));

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));
    }
}