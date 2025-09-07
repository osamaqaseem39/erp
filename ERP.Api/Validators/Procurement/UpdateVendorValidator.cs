using ERP.Shared.DTOs.Procurement;
using FluentValidation;

namespace ERP.Api.Validators.Procurement;

public class UpdateVendorValidator : BaseValidator<UpdateVendorDto>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.VendorName)
            .NotEmpty().WithMessage("Vendor name is required")
            .MaximumLength(200).WithMessage("Vendor name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.VendorName));

        RuleFor(x => x.VendorCode)
            .NotEmpty().WithMessage("Vendor code is required")
            .MaximumLength(50).WithMessage("Vendor code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.VendorCode));

        RuleFor(x => x.VendorType)
            .NotEmpty().WithMessage("Vendor type is required")
            .MaximumLength(50).WithMessage("Vendor type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.VendorType));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100).WithMessage("Payment terms cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0")
            .When(x => x.CreditLimit.HasValue);
    }
}