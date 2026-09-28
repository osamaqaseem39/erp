using ERP.Shared.DTOs.Sales;
using FluentValidation;

namespace ERP.Api.Validators.Sales;

public class CreatePOSTransactionValidator : BaseValidator<CreatePOSTransactionDto>
{
    public CreatePOSTransactionValidator()
    {
        RuleFor(x => x.TransactionNumber)
            .NotEmpty().WithMessage("Transaction number is required")
            .MaximumLength(50).WithMessage("Transaction number cannot exceed 50 characters");

        RuleFor(x => x.POSSessionId)
            .NotEmpty().WithMessage("POS session ID is required");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters");

        RuleFor(x => x.SubTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Sub total must be greater than or equal to 0");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than or equal to 0");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount must be greater than or equal to 0");

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("Amount paid must be greater than or equal to 0");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one transaction line is required")
            .Must(lines => lines != null && lines.Any()).WithMessage("At least one transaction line is required");
    }
}

