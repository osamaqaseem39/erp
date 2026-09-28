using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateCustomerGroupValidator : BaseValidator<CreateCustomerGroupDto>
{
    public CreateCustomerGroupValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateCustomerGroupValidator : BaseValidator<UpdateCustomerGroupDto>
{
    public UpdateCustomerGroupValidator()
    {
        RuleFor(x => x.GroupName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.GroupName));
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue);
    }
}

public class CreatePriceListValidator : BaseValidator<CreatePriceListDto>
{
    public CreatePriceListValidator()
    {
        RuleFor(x => x.PriceListName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValidTo).GreaterThanOrEqualTo(x => x.ValidFrom).When(x => x.ValidTo.HasValue);
    }
}

public class UpdatePriceListValidator : BaseValidator<UpdatePriceListDto>
{
    public UpdatePriceListValidator()
    {
        RuleFor(x => x.PriceListName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.PriceListName));
    }
}

public class CreateSalesTerritoryValidator : BaseValidator<CreateSalesTerritoryDto>
{
    public CreateSalesTerritoryValidator()
    {
        RuleFor(x => x.TerritoryName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateSalesTerritoryValidator : BaseValidator<UpdateSalesTerritoryDto>
{
    public UpdateSalesTerritoryValidator()
    {
        RuleFor(x => x.TerritoryName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.TerritoryName));
    }
}

public class CreateReturnOrderValidator : BaseValidator<CreateReturnOrderDto>
{
    public CreateReturnOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
    }
}

public class UpdateReturnOrderValidator : BaseValidator<UpdateReturnOrderDto>
{
    public UpdateReturnOrderValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Reason));
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0).When(x => x.TotalAmount.HasValue);
    }
}
