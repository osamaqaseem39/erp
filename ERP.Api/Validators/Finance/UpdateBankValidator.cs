using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateBankValidator : BaseValidator<UpdateBankDto>
{
    public UpdateBankValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Bank ID is required");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required")
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters");

        RuleFor(x => x.BankCode)
            .NotEmpty().WithMessage("Bank code is required")
            .MaximumLength(20).WithMessage("Bank code cannot exceed 20 characters");

        RuleFor(x => x.SwiftCode)
            .MaximumLength(11).WithMessage("SWIFT code cannot exceed 11 characters")
            .When(x => !string.IsNullOrEmpty(x.SwiftCode));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Website)
            .Must(BeAValidUrl).WithMessage("Invalid website URL format")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("Updated by is required");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}