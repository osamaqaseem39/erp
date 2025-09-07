using ERP.Shared.DTOs.HRM;
using FluentValidation;

namespace ERP.Api.Validators.HRM;

public class EmployeeCreateValidator : BaseValidator<CreateEmployeeDto>
{
    public EmployeeCreateValidator()
    {
        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required")
            .MaximumLength(50).WithMessage("Employee code cannot exceed 50 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.WorkEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.WorkEmail));

        RuleFor(x => x.PersonalEmail)
            .EmailAddress().WithMessage("Invalid personal email format")
            .When(x => !string.IsNullOrEmpty(x.PersonalEmail));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.DateHired)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date hired cannot be in the future")
            .When(x => x.DateHired.HasValue);
    }
}

public class EmployeeUpdateValidator : BaseValidator<UpdateEmployeeDto>
{
    public EmployeeUpdateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.WorkEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.WorkEmail));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.DateTerminated)
            .GreaterThan(x => x.DateHired).WithMessage("Date terminated must be after date hired")
            .When(x => x.DateTerminated.HasValue && x.DateHired.HasValue);
    }
}