using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class UpdateBillValidator : BaseValidator<UpdateBillDto>
{
    public UpdateBillValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Bill ID is required");

        RuleFor(x => x.BillNumber)
            .NotEmpty().WithMessage("Bill number is required")
            .MaximumLength(50).WithMessage("Bill number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.BillNumber));

        RuleFor(x => x.BillDate)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Bill date cannot be in the future")
            .When(x => x.BillDate.HasValue);

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.BillDate).WithMessage("Due date must be after bill date")
            .When(x => x.DueDate.HasValue && x.BillDate.HasValue);

        RuleFor(x => x.SubTotal)
            .GreaterThan(0).WithMessage("Sub total must be greater than 0")
            .When(x => x.SubTotal.HasValue);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0")
            .When(x => x.TaxAmount.HasValue);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0")
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));

        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0")
            .When(x => x.ExchangeRate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}