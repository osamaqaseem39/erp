using ERP.Shared.DTOs.Sales;
using FluentValidation;

namespace ERP.Api.Validators.Sales;

public class CreatePOSSessionValidator : BaseValidator<CreatePOSSessionDto>
{
    public CreatePOSSessionValidator()
    {
        RuleFor(x => x.SessionNumber)
            .NotEmpty().WithMessage("Session number is required")
            .MaximumLength(50).WithMessage("Session number cannot exceed 50 characters");

        RuleFor(x => x.CashierId)
            .NotEmpty().WithMessage("Cashier ID is required");

        RuleFor(x => x.TerminalId)
            .NotEmpty().WithMessage("Terminal ID is required");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required");

        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Opening balance must be greater than or equal to 0");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

