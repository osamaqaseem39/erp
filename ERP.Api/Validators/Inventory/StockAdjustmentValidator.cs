using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class StockAdjustmentValidator : BaseValidator<StockAdjustmentDto>
{
    public StockAdjustmentValidator()
    {
        RuleFor(x => x.AdjustmentNumber)
            .NotEmpty().WithMessage("Adjustment number is required")
            .MaximumLength(50).WithMessage("Adjustment number cannot exceed 50 characters");

        RuleFor(x => x.AdjustmentDate)
            .NotEmpty().WithMessage("Adjustment date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Adjustment date cannot be in the future");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required");

        RuleFor(x => x.AdjustmentType)
            .NotEmpty().WithMessage("Adjustment type is required")
            .Must(type => new[] { "Physical", "System", "Damage", "Theft", "Expired" }.Contains(type))
            .WithMessage("Adjustment type must be Physical, System, Damage, Theft, or Expired");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Draft", "Pending", "Approved", "Rejected", "Completed" }.Contains(status))
            .WithMessage("Status must be Draft, Pending, Approved, Rejected, or Completed");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one adjustment detail is required")
            .Must(details => details != null && details.Any())
            .WithMessage("At least one adjustment detail is required");
    }
}