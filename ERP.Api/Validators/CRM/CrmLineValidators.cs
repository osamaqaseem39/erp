using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateQuoteLineValidator : AbstractValidator<CreateQuoteLineDto>
{
    public CreateQuoteLineValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to 0")
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class CreateSalesOrderLineValidator : AbstractValidator<CreateSalesOrderLineDto>
{
    public CreateSalesOrderLineValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to 0")
            .When(x => x.TaxRate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
