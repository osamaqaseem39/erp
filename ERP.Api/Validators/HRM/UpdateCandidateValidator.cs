using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateCandidateValidator : BaseValidator<UpdateCandidateDto>
{
    public UpdateCandidateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Candidate ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today.AddYears(-16)).WithMessage("Candidate must be at least 16 years old")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Experience)
            .MaximumLength(200).WithMessage("Experience cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Experience));

        RuleFor(x => x.Education)
            .MaximumLength(200).WithMessage("Education cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Education));

        RuleFor(x => x.Skills)
            .MaximumLength(1000).WithMessage("Skills cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Skills));

        RuleFor(x => x.ExpectedSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Expected salary must be greater than or equal to 0")
            .When(x => x.ExpectedSalary.HasValue);

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}