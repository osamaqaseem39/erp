using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateDepartmentValidator : BaseValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}