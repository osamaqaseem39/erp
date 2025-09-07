using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class UpdateTrainingValidator : BaseValidator<UpdateTrainingDto>
{
    public UpdateTrainingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("Max participants must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Max participants cannot exceed 1000");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Planned", "In Progress", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be one of: Planned, In Progress, Completed, Cancelled");
    }
}