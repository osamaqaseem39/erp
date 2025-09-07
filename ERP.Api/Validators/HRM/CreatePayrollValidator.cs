using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreatePayrollValidator : BaseValidator<CreatePayrollDto>
{
    public CreatePayrollValidator()
    {
        RuleFor(x => x.PayPeriodStart)
            .NotEmpty().WithMessage("Pay period start is required");

        RuleFor(x => x.PayPeriodEnd)
            .NotEmpty().WithMessage("Pay period end is required")
            .GreaterThan(x => x.PayPeriodStart).WithMessage("Pay period end must be after start");

        RuleFor(x => x.PayDate)
            .NotEmpty().WithMessage("Pay date is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency cannot exceed 3 characters");
    }
}