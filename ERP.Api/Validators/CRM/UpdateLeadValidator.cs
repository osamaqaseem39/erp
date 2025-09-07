using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class UpdateLeadValidator : BaseValidator<UpdateLeadDto>
{
    public UpdateLeadValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

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
            .NotEmpty().WithMessage("Lead source is required")
            .MaximumLength(100).WithMessage("Lead source cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LeadSource));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .MaximumLength(50).WithMessage("Priority cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.EstimatedValue)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated value must be greater than or equal to 0")
            .When(x => x.EstimatedValue.HasValue);

        RuleFor(x => x.LastContactDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Last contact date cannot be in the future")
            .When(x => x.LastContactDate.HasValue);
    }
}