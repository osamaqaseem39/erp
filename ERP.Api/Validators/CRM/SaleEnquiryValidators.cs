using ERP.Shared.DTOs.CRM;
using FluentValidation;

namespace ERP.Api.Validators.CRM;

public class CreateSaleEnquiryValidator : BaseValidator<CreateSaleEnquiryDto>
{
    public CreateSaleEnquiryValidator() => SaleEnquiryRules.Apply(this);
}

public class UpdateSaleEnquiryValidator : BaseValidator<UpdateSaleEnquiryDto>
{
    public UpdateSaleEnquiryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Enquiry ID is required");
        SaleEnquiryRules.Apply(this);
    }
}

public class RejectSaleEnquiryValidator : BaseValidator<RejectSaleEnquiryDto>
{
    public RejectSaleEnquiryValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters");
    }
}

internal static class SaleEnquiryRules
{
    public static void Apply<T>(AbstractValidator<T> v) where T : CreateSaleEnquiryDto
    {
        v.RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        v.RuleFor(x => x.EnquiryDate)
            .LessThanOrEqualTo(_ => DateTime.Today.AddDays(1)).WithMessage("Enquiry date cannot be in the future")
            .When(x => x.EnquiryDate.HasValue);

        v.RuleFor(x => x.ReferenceNo)
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters");

        v.RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Add at least one line");

        v.RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            line.RuleFor(l => l.ItemDescription)
                .NotEmpty().WithMessage("Pick an item or enter a description")
                .When(l => l.ItemId is null);
            line.RuleFor(l => l.Ups)
                .GreaterThan(0).WithMessage("Ups must be greater than 0")
                .When(l => l.Ups.HasValue);
        });
    }
}
