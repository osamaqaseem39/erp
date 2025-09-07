using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateBankAccountValidator : BaseValidator<CreateBankAccountDto>
{
    public CreateBankAccountValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(100).WithMessage("Account name cannot exceed 100 characters");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required")
            .MaximumLength(50).WithMessage("Account type cannot exceed 50 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-character code");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required");

        RuleFor(x => x.BankId)
            .NotEmpty().WithMessage("Bank ID is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required");
    }
}