using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateOpportunityValidator : BaseValidator<CreateOpportunityDto>
{
    public CreateOpportunityValidator()
    {
        RuleFor(x => x.OpportunityName)
            .NotEmpty().WithMessage("Opportunity name is required")
            .MaximumLength(200).WithMessage("Opportunity name cannot exceed 200 characters");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.LeadId)
            .NotEmpty().WithMessage("Lead ID is required")
            .When(x => x.LeadId.HasValue);


        RuleFor(x => x.Stage)
            .NotEmpty().WithMessage("Stage is required")
            .MaximumLength(50).WithMessage("Stage cannot exceed 50 characters");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100")
            .When(x => x.Probability.HasValue);

        RuleFor(x => x.EstimatedValue)
            .GreaterThan(0).WithMessage("Estimated value must be greater than 0")
            .When(x => x.EstimatedValue.HasValue);

        RuleFor(x => x.ExpectedCloseDate)
            .NotEmpty().WithMessage("Expected close date is required")
            .GreaterThan(DateTime.Today).WithMessage("Expected close date must be in the future");

        RuleFor(x => x.ActualCloseDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Actual close date cannot be in the future")
            .When(x => x.ActualCloseDate.HasValue);

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Source cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Source));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");
    }
}