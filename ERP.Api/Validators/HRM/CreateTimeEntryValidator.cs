using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreateTimeEntryValidator : BaseValidator<CreateTimeEntryDto>
{
    public CreateTimeEntryValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.WorkDate)
            .NotEmpty().WithMessage("Work date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Work date cannot be in the future");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .When(x => x.StartTime.HasValue);

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time")
            .When(x => x.EndTime.HasValue && x.StartTime.HasValue);

        RuleFor(x => x.Hours)
            .GreaterThan(0).WithMessage("Hours must be greater than 0")
            .LessThanOrEqualTo(24).WithMessage("Hours cannot exceed 24")
            .When(x => x.Hours.HasValue);

        RuleFor(x => x.HoursWorked)
            .GreaterThan(0).WithMessage("Hours worked must be greater than 0")
            .LessThanOrEqualTo(24).WithMessage("Hours worked cannot exceed 24")
            .When(x => x.HoursWorked.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Hourly rate must be greater than 0")
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}