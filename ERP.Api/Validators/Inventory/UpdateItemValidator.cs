using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class UpdateItemValidator : BaseValidator<UpdateItemDto>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID is required");

        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item code is required")
            .MaximumLength(50).WithMessage("Item code cannot exceed 50 characters");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("Item name is required")
            .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ItemCategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.UnitOfMeasureId)
            .NotEmpty().WithMessage("Unit of measure is required");

        RuleFor(x => x.ItemType)
            .NotEmpty().WithMessage("Item type is required")
            .MaximumLength(50).WithMessage("Item type cannot exceed 50 characters");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be greater than or equal to 0");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be greater than or equal to 0");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be greater than or equal to 0");

        RuleFor(x => x.ReorderQuantity)
            .GreaterThan(0).WithMessage("Reorder quantity must be greater than 0");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Active status is required");
    }
}