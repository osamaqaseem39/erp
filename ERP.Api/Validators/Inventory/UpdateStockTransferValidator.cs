using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class UpdateStockTransferValidator : BaseValidator<UpdateStockTransferDto>
{
    public UpdateStockTransferValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.TransferNumber)
            .NotEmpty().WithMessage("Transfer number is required")
            .MaximumLength(50).WithMessage("Transfer number cannot exceed 50 characters");

        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("From warehouse is required");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("To warehouse is required")
            .NotEqual(x => x.FromWarehouseId).WithMessage("From warehouse and To warehouse cannot be the same");

        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("Transfer date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Transfer date cannot be in the future");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Draft", "Pending", "Approved", "In Transit", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be one of: Draft, Pending, Approved, In Transit, Completed, Cancelled");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");

        RuleForEach(x => x.Details)
            .SetValidator(new UpdateStockTransferLineValidator());
    }
}

public class UpdateStockTransferLineValidator : BaseValidator<UpdateStockTransferDetailDto>
{
    public UpdateStockTransferLineValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Quantity cannot exceed 1,000,000");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}