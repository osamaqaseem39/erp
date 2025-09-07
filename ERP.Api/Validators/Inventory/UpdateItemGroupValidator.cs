using ERP.Shared.DTOs.Inventory;
using FluentValidation;

namespace ERP.Api.Validators.Inventory;

public class UpdateItemGroupValidator : BaseValidator<UpdateItemGroupDto>
{
    public UpdateItemGroupValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item group ID is required");

        RuleFor(x => x.GroupCode)
            .NotEmpty().WithMessage("Group code is required")
            .MaximumLength(20).WithMessage("Group code cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.GroupCode));

        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(100).WithMessage("Group name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.GroupName));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}