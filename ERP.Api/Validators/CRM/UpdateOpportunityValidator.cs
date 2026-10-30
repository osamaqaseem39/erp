using ERP.Application.CRM;
using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class UpdateOpportunityValidator : BaseValidator<UpdateOpportunityDto>
{
    public UpdateOpportunityValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required");

        RuleFor(x => x.OpportunityName)
            .NotEmpty().WithMessage("Opportunity name is required")
            .MaximumLength(200).WithMessage("Opportunity name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.OpportunityName));

        RuleFor(x => x.Stage)
            .Must(OpportunityStages.IsOpen!).WithMessage($"Stage must be one of: {string.Join(", ", OpportunityStages.Open.Keys)}")
            .When(x => !string.IsNullOrEmpty(x.Stage));

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100")
            .When(x => x.Probability.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Deal value cannot be negative")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.EstimatedValue)
            .GreaterThanOrEqualTo(0).WithMessage("Deal value cannot be negative")
            .When(x => x.EstimatedValue.HasValue);

        RuleFor(x => x.ExpectedCloseDate)
            .NotEmpty().WithMessage("Expected close date is required");

        RuleFor(x => x.ActualCloseDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Actual close date cannot be in the future")
            .When(x => x.ActualCloseDate.HasValue);

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Source cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Source));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

    }
}