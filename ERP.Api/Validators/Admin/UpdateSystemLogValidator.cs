using ERP.Shared.DTOs.Admin;
using FluentValidation;

namespace ERP.Api.Validators.Admin;

public class UpdateSystemLogValidator : AbstractValidator<UpdateSystemLogDto>
{
    public UpdateSystemLogValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.LogLevel)
            .NotEmpty().WithMessage("Log level is required")
            .MaximumLength(20).WithMessage("Log level cannot exceed 20 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Source cannot exceed 100 characters");
    }
}