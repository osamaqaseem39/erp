using ERP.Shared.DTOs.Manufacturing;
using FluentValidation;

namespace ERP.Api.Validators.Manufacturing;

public class CreateProductionOrderValidator : BaseValidator<CreateProductionOrderDto>
{
    public CreateProductionOrderValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

    }
}