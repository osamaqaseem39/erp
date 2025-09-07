using ERP.Shared.DTOs.Procurement;
using FluentValidation;

namespace ERP.Api.Validators.Procurement;

public class CreateVendorBankAccountValidator : BaseValidator<CreateVendorBankAccountDto>
{
    public CreateVendorBankAccountValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required")
            .MaximumLength(200).WithMessage("Bank name cannot exceed 200 characters");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.IBAN)
            .NotEmpty().WithMessage("IBAN is required")
            .MaximumLength(50).WithMessage("IBAN cannot exceed 50 characters");

        RuleFor(x => x.SwiftCode)
            .NotEmpty().WithMessage("SWIFT code is required")
            .MaximumLength(20).WithMessage("SWIFT code cannot exceed 20 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters");

        RuleFor(x => x.IsPrimary)
            .NotNull().WithMessage("IsPrimary status is required");
    }
}