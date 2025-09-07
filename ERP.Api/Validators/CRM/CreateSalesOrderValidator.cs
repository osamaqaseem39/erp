using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateSalesOrderValidator : BaseValidator<CreateSalesOrderDto>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required")
            .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.QuoteId)
            .NotEmpty().WithMessage("Quote ID is required")
            .When(x => x.QuoteId.HasValue);

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Order date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Order date cannot be in the future");

        RuleFor(x => x.RequiredDate)
            .NotEmpty().WithMessage("Required date is required")
            .GreaterThan(x => x.OrderDate).WithMessage("Required date must be after order date");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage("Sub total must be greater than 0");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}