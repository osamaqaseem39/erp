using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateBillValidator : BaseValidator<CreateBillDto>
{
    public CreateBillValidator()
    {
        RuleFor(x => x.BillNumber)
            .NotEmpty().WithMessage("Bill number is required")
            .MaximumLength(50).WithMessage("Bill number cannot exceed 50 characters");

        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.BillDate)
            .NotEmpty().WithMessage("Bill date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Bill date cannot be in the future");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThan(x => x.BillDate).WithMessage("Due date must be after bill date");

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage("Sub total must be greater than 0");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0")
            .When(x => x.ExchangeRate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}