using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateInterviewValidator : BaseValidator<UpdateInterviewDto>
{
    public UpdateInterviewValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required")
            .When(x => x.JobApplicationId.HasValue);

        RuleFor(x => x.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required")
            .When(x => x.CandidateId.HasValue);

        RuleFor(x => x.InterviewerId)
            .NotEmpty().WithMessage("Interviewer ID is required")
            .When(x => x.InterviewerId.HasValue);

        RuleFor(x => x.InterviewDate)
            .GreaterThan(DateTime.Now).WithMessage("Interview date must be in the future")
            .When(x => x.InterviewDate.HasValue);

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTime.Now).WithMessage("Scheduled date must be in the future")
            .When(x => x.ScheduledDate.HasValue);

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(480).WithMessage("Duration cannot exceed 8 hours (480 minutes)")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.InterviewType)
            .MaximumLength(50).WithMessage("Interview type cannot exceed 50 characters")
            .Must(type => new[] { "InPerson", "Video", "Phone" }.Contains(type))
            .WithMessage("Interview type must be InPerson, Video, or Phone")
            .When(x => !string.IsNullOrEmpty(x.InterviewType));

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .Must(status => new[] { "Scheduled", "Completed", "Cancelled", "Rescheduled" }.Contains(status))
            .WithMessage("Status must be Scheduled, Completed, Cancelled, or Rescheduled")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Feedback)
            .MaximumLength(2000).WithMessage("Feedback cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Feedback));

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}