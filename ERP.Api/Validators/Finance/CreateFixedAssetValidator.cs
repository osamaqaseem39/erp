using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Validators.Finance;

public class CreateFixedAssetValidator : AbstractValidator<CreateFixedAssetDto>
{
    public CreateFixedAssetValidator()
    {
        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(200).WithMessage("Asset name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("Purchase date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Purchase date cannot be in the future");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required")
            .MaximumLength(50).WithMessage("Asset type cannot exceed 50 characters");

        RuleFor(x => x.GLAccountId)
            .NotEmpty().WithMessage("GL account is required");

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location cannot exceed 100 characters");
    }
}