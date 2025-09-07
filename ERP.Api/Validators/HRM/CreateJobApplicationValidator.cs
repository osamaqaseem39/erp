using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreateJobApplicationValidator : BaseValidator<CreateJobApplicationDto>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.JobOpeningId)
            .NotEmpty().WithMessage("Job opening ID is required");

        RuleFor(x => x.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required");

        RuleFor(x => x.AppliedDate)
            .NotEmpty().WithMessage("Application date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Application date cannot be in the future");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.CoverLetter)
            .MaximumLength(2000).WithMessage("Cover letter cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CoverLetter));


        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}