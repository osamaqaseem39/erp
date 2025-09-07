using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class StockTransferValidator : BaseValidator<StockTransferDto>
{
    public StockTransferValidator()
    {
        RuleFor(x => x.TransferNumber)
            .NotEmpty().WithMessage("Transfer number is required")
            .MaximumLength(50).WithMessage("Transfer number cannot exceed 50 characters");

        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("Transfer date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Transfer date cannot be in the future");

        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("From warehouse ID is required");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("To warehouse ID is required")
            .NotEqual(x => x.FromWarehouseId).WithMessage("From warehouse and To warehouse cannot be the same");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Draft", "Pending", "Approved", "InTransit", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be Draft, Pending, Approved, InTransit, Completed, or Cancelled");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one transfer detail is required")
            .Must(details => details != null && details.Any())
            .WithMessage("At least one transfer detail is required");
    }
}