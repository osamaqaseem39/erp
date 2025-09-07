using FluentValidation.Results;

namespace ERP.Api.Validators;

public interface IValidator<T>
{
    Task<ValidationResult> ValidateAsync(T model);
    ValidationResult Validate(T model);
}