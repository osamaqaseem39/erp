using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class UpdateSalesOrderValidator : BaseValidator<UpdateSalesOrderDto>
{
    public UpdateSalesOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Sales order ID is required");

        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.OrderNumber));

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Order date cannot be in the future")
            .When(x => x.OrderDate.HasValue);

        RuleFor(x => x.RequiredDate)
            .GreaterThan(x => x.OrderDate).WithMessage("Required date must be after order date")
            .When(x => x.RequiredDate.HasValue && x.OrderDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage("Sub total must be greater than 0")
            .When(x => x.SubTotal.HasValue);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0")
            .When(x => x.TaxAmount.HasValue);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0")
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleForEach(x => x.SalesOrderLines)
            .SetValidator(new CreateSalesOrderLineValidator())
            .When(x => x.SalesOrderLines.Count > 0);
    }
}