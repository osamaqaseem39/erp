using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateLeaveRequestValidator : BaseValidator<UpdateLeaveRequestDto>
{
    public UpdateLeaveRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .IsInEnum().WithMessage("Status must be a valid value");
    }
}