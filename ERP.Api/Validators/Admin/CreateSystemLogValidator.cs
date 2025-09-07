using ERP.Shared.DTOs.Admin;
using FluentValidation;

namespace ERP.Api.Validators.Admin;

public class CreateSystemLogValidator : AbstractValidator<CreateSystemLogDto>
{
    public CreateSystemLogValidator()
    {
        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid log level");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Source cannot exceed 100 characters");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}