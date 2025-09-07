using ERP.Shared.DTOs.Admin;
using FluentValidation;

namespace ERP.Api.Validators.Admin;

public class CreateAuditLogValidator : AbstractValidator<CreateAuditLogDto>
{
    public CreateAuditLogValidator()
    {
        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required")
            .MaximumLength(100).WithMessage("Entity name cannot exceed 100 characters");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .MaximumLength(50).WithMessage("Action cannot exceed 50 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}