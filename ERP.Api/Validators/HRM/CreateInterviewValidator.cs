using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreateInterviewValidator : BaseValidator<CreateInterviewDto>
{
    public CreateInterviewValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required");

        RuleFor(x => x.InterviewerId)
            .NotEmpty().WithMessage("Interviewer ID is required");

        RuleFor(x => x.InterviewDate)
            .NotEmpty().WithMessage("Interview date is required")
            .GreaterThan(DateTime.Now).WithMessage("Interview date must be in the future");

        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("Scheduled date is required")
            .GreaterThan(DateTime.Now).WithMessage("Scheduled date must be in the future");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(480).WithMessage("Duration cannot exceed 8 hours (480 minutes)");

        RuleFor(x => x.InterviewType)
            .NotEmpty().WithMessage("Interview type is required")
            .MaximumLength(50).WithMessage("Interview type cannot exceed 50 characters")
            .Must(type => new[] { "InPerson", "Video", "Phone" }.Contains(type))
            .WithMessage("Interview type must be InPerson, Video, or Phone");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .Must(status => new[] { "Scheduled", "Completed", "Cancelled", "Rescheduled" }.Contains(status))
            .WithMessage("Status must be Scheduled, Completed, Cancelled, or Rescheduled");

        RuleFor(x => x.Feedback)
            .MaximumLength(2000).WithMessage("Feedback cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Feedback));

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}