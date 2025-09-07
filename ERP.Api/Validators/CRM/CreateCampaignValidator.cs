using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateCampaignValidator : BaseValidator<CreateCampaignDto>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.CampaignName)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters");

        RuleFor(x => x.CampaignType)
            .NotEmpty().WithMessage("Campaign type is required")
            .MaximumLength(50).WithMessage("Campaign type cannot exceed 50 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be greater than or equal to 0");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters");
    }
}