using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateLeadValidator : BaseValidator<CreateLeadDto>
{
    public CreateLeadValidator()
    {
        // LeadCode is issued by the server sequence; whatever the client sends is ignored.

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.LeadSource)
            .Must(LeadRules.IsSource).WithMessage($"Lead source must be one of: {string.Join(", ", LeadRules.Sources)}")
            .When(x => !string.IsNullOrEmpty(x.LeadSource));

        // Status is server-controlled (always starts New), so it is not validated here.

        RuleFor(x => x.Priority)
            .Must(LeadRules.IsPriority).WithMessage($"Priority must be one of: {string.Join(", ", LeadRules.Priorities)}")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.LeadScore)
            .InclusiveBetween(0, 100).WithMessage("Lead score must be between 0 and 100")
            .When(x => x.LeadScore.HasValue);

        RuleFor(x => x.EstimatedValue)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated value must be greater than or equal to 0")
            .When(x => x.EstimatedValue.HasValue);

        RuleFor(x => x.LastContactDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Last contact date cannot be in the future")
            .When(x => x.LastContactDate.HasValue);
    }
}

public class LoseLeadValidator : BaseValidator<LoseLeadDto>
{
    public LoseLeadValidator()
    {
        RuleFor(x => x.LostReason)
            .NotEmpty().WithMessage("Lost reason is required")
            .Must(LeadRules.IsLostReason).WithMessage($"Lost reason must be one of: {string.Join(", ", LeadRules.LostReasons)}");

        RuleFor(x => x.LostNote)
            .MaximumLength(1000).WithMessage("Note cannot exceed 1000 characters");
    }
}

public class ConvertLeadToOpportunityValidator : BaseValidator<ConvertLeadToOpportunityDto>
{
    public ConvertLeadToOpportunityValidator()
    {
        RuleFor(x => x.OpportunityName).MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue).WithMessage("Amount cannot be negative");
        RuleFor(x => x.Probability).InclusiveBetween(0, 100).When(x => x.Probability.HasValue).WithMessage("Probability must be 0–100");
        RuleFor(x => x.ExpectedCloseDate).GreaterThanOrEqualTo(DateTime.Today).When(x => x.ExpectedCloseDate.HasValue)
            .WithMessage("Expected close date cannot be in the past");
    }
}

public static class LeadRules
{
    public static readonly string[] LostReasons = ["Price", "Competitor", "No budget", "No response", "Not a fit", "Timing", "Other"];
    public static bool IsLostReason(string? value) => LostReasons.Contains(value, StringComparer.OrdinalIgnoreCase);

    public static readonly string[] Sources = ["Website", "Referral", "Social Media", "Email Campaign", "Cold Call", "Event", "Other"];
    public static readonly string[] Priorities = ["Low", "Medium", "High", "Urgent"];

    public static bool IsSource(string? value) => Sources.Contains(value, StringComparer.OrdinalIgnoreCase);
    public static bool IsPriority(string? value) => Priorities.Contains(value, StringComparer.OrdinalIgnoreCase);
}
