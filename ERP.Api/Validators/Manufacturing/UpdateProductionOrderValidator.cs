using ERP.Shared.DTOs.Manufacturing;
using FluentValidation;

namespace ERP.Api.Validators.Manufacturing;

public class UpdateProductionOrderValidator : BaseValidator<UpdateProductionOrderDto>
{
    public UpdateProductionOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Production order ID is required");

        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.OrderNumber));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}