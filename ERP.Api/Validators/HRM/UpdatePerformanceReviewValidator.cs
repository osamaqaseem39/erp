using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdatePerformanceReviewValidator : BaseValidator<UpdatePerformanceReviewDto>
{
    public UpdatePerformanceReviewValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required")
            .When(x => x.ReviewerId.HasValue);

        RuleFor(x => x.ReviewPeriodStart)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Review period start date cannot be in the future")
            .When(x => x.ReviewPeriodStart.HasValue);

        RuleFor(x => x.ReviewPeriodEnd)
            .GreaterThan(x => x.ReviewPeriodStart).WithMessage("Review period end date must be after start date")
            .When(x => x.ReviewPeriodEnd.HasValue && x.ReviewPeriodStart.HasValue);

        RuleFor(x => x.ReviewDate)
            .GreaterThanOrEqualTo(x => x.ReviewPeriodEnd).WithMessage("Review date must be on or after review period end date")
            .When(x => x.ReviewDate.HasValue && x.ReviewPeriodEnd.HasValue);

        RuleFor(x => x.OverallRating)
            .InclusiveBetween(1, 5).WithMessage("Overall rating must be between 1 and 5")
            .When(x => x.OverallRating.HasValue);

        RuleFor(x => x.GoalsAchieved)
            .MaximumLength(2000).WithMessage("Goals achieved cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.GoalsAchieved));

        RuleFor(x => x.GoalsNotAchieved)
            .MaximumLength(2000).WithMessage("Goals not achieved cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.GoalsNotAchieved));

        RuleFor(x => x.Strengths)
            .MaximumLength(2000).WithMessage("Strengths cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Strengths));

        RuleFor(x => x.AreasForImprovement)
            .MaximumLength(2000).WithMessage("Areas for improvement cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.AreasForImprovement));

        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .Must(status => new[] { "Draft", "InProgress", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be Draft, InProgress, Completed, or Cancelled")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}