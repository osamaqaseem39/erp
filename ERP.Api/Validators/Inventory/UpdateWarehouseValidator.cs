using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class UpdateWarehouseValidator : BaseValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");

        RuleFor(x => x.WarehouseCode)
            .NotEmpty().WithMessage("Warehouse code is required")
            .MaximumLength(20).WithMessage("Warehouse code cannot exceed 20 characters");

        RuleFor(x => x.WarehouseName)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(100).WithMessage("Warehouse name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.AddressLine1)
            .MaximumLength(500).WithMessage("Address line 1 cannot exceed 500 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(500).WithMessage("Address line 2 cannot exceed 500 characters");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Active status is required");
    }
}