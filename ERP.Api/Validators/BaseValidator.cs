using FluentValidation;
using FluentValidation.Results;

namespace ERP.Api.Validators;

public abstract class BaseValidator<T> : AbstractValidator<T>, IValidator<T>
{
    public async Task<ValidationResult> ValidateAsync(T model)
    {
        return await base.ValidateAsync(model);
    }

    public new ValidationResult Validate(T model)
    {
        return base.Validate(model);
    }
}