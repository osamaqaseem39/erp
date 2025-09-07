using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class UpdateActivityValidator : BaseValidator<UpdateActivityDto>
{
    public UpdateActivityValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters");

        RuleFor(x => x.ActivityType)
            .NotEmpty().WithMessage("Activity type is required")
            .MaximumLength(50).WithMessage("Activity type cannot exceed 50 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .IsInEnum().WithMessage("Priority must be a valid value");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");
    }
}