using ERP.Shared.DTOs.Core;
using FluentValidation;

namespace ERP.Api.Validators;

public class LoginValidator : BaseValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.PrimaryEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}

public class RefreshTokenValidator : BaseValidator<RefreshTokenDto>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}