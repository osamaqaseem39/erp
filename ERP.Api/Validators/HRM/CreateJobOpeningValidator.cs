using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreateJobOpeningValidator : BaseValidator<CreateJobOpeningDto>
{
    public CreateJobOpeningValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title cannot exceed 200 characters");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters");

        RuleFor(x => x.EmploymentType)
            .NotEmpty().WithMessage("Employment type is required")
            .MaximumLength(50).WithMessage("Employment type cannot exceed 50 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Requirements)
            .MaximumLength(2000).WithMessage("Requirements cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Requirements));

        RuleFor(x => x.Responsibilities)
            .MaximumLength(2000).WithMessage("Responsibilities cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Responsibilities));

        RuleFor(x => x.ExperienceLevel)
            .MaximumLength(100).WithMessage("Experience level cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ExperienceLevel));

        RuleFor(x => x.SalaryRange)
            .MaximumLength(100).WithMessage("Salary range cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SalaryRange));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.PostedDate)
            .NotEmpty().WithMessage("Posted date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Posted date cannot be in the future");

        RuleFor(x => x.ClosingDate)
            .GreaterThan(x => x.PostedDate).WithMessage("Closing date must be after posted date")
            .When(x => x.ClosingDate.HasValue);
    }
}