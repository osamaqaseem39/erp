using ERP.Shared.DTOs.Sales;
using FluentValidation;

namespace ERP.Api.Validators.Sales;

public class CreateSalesInvoiceValidator : BaseValidator<CreateSalesInvoiceDto>
{
    public CreateSalesInvoiceValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThanOrEqualTo(x => x.InvoiceDate).WithMessage("Due date must be greater than or equal to invoice date");

        RuleFor(x => x.SubTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Sub total must be greater than or equal to 0");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Terms)
            .MaximumLength(500).WithMessage("Terms cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Terms));

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one invoice line is required")
            .Must(lines => lines != null && lines.Any()).WithMessage("At least one invoice line is required");
    }
}

