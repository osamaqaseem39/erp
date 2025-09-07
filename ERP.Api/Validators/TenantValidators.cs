using ERP.Shared.DTOs.Core;
using FluentValidation;

namespace ERP.Api.Validators;

public class CreateTenantValidator : BaseValidator<CreateTenantDto>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required")
            .MaximumLength(100).WithMessage("Domain cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$").WithMessage("Domain must be valid");

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Active", "Suspended", "Deleted" }.Contains(status))
            .WithMessage("Status must be Active, Suspended, or Deleted");
    }
}

public class UpdateTenantValidator : BaseValidator<UpdateTenantDto>
{
    public UpdateTenantValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required")
            .MaximumLength(100).WithMessage("Domain cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$").WithMessage("Domain must be valid");

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Active", "Suspended", "Deleted" }.Contains(status))
            .WithMessage("Status must be Active, Suspended, or Deleted");
    }
}