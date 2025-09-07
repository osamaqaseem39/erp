using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class CreateBenefitValidator : BaseValidator<CreateBenefitDto>
{
    public CreateBenefitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Benefit name is required")
            .MaximumLength(100).WithMessage("Benefit name cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Benefit type is required")
            .MaximumLength(50).WithMessage("Benefit type cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0")
            .When(x => x.Cost.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive status is required");
    }
}