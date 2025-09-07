using ERP.Shared.DTOs.Projects;
using FluentValidation;

namespace ERP.Api.Validators.Projects;

public class UpdateProjectValidator : BaseValidator<UpdateProjectDto>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ProjectName));

        RuleFor(x => x.ProjectCode)
            .NotEmpty().WithMessage("Project code is required")
            .MaximumLength(50).WithMessage("Project code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ProjectCode));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));


        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .MaximumLength(50).WithMessage("Priority cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be greater than or equal to 0")
            .When(x => x.Budget.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}