using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class CreateWarehouseValidator : BaseValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.WarehouseCode)
            .NotEmpty().WithMessage("Warehouse code is required")
            .MaximumLength(20).WithMessage("Warehouse code cannot exceed 20 characters");

        RuleFor(x => x.WarehouseName)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(100).WithMessage("Warehouse name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(500).WithMessage("Address line 1 cannot exceed 500 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(500).WithMessage("Address line 2 cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Active status is required");
    }
}