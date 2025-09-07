using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateJobApplicationValidator : BaseValidator<UpdateJobApplicationDto>
{
    public UpdateJobApplicationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.AppliedDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Applied date cannot be in the future")
            .When(x => x.AppliedDate.HasValue);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.CoverLetter)
            .MaximumLength(2000).WithMessage("Cover letter cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.CoverLetter));


        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}