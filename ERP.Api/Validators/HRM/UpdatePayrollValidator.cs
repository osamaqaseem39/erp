using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdatePayrollValidator : BaseValidator<UpdatePayrollDto>
{
    public UpdatePayrollValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.PayPeriodStart)
            .NotEmpty().WithMessage("Pay period start is required");

        RuleFor(x => x.PayPeriodEnd)
            .NotEmpty().WithMessage("Pay period end is required")
            .GreaterThan(x => x.PayPeriodStart).WithMessage("Pay period end must be after start");

        RuleFor(x => x.PayDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Pay date cannot be in the past")
            .When(x => x.PayDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency cannot exceed 3 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}