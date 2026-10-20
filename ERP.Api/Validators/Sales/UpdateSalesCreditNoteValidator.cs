using ERP.Shared.DTOs.Sales;
using FluentValidation;

namespace ERP.Api.Validators.Sales;

public class UpdateSalesCreditNoteValidator : BaseValidator<UpdateSalesCreditNoteDto>
{
    public UpdateSalesCreditNoteValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.CreditNoteNumber)
            .NotEmpty().WithMessage("Credit note number is required")
            .MaximumLength(50);

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.OriginalSalesInvoiceId)
            .NotEmpty().WithMessage("Original sales invoice ID is required");

        RuleFor(x => x.CreditNoteDate)
            .NotEmpty().WithMessage("Credit note date is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one credit note line is required");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.TaxRate).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Description).MaximumLength(500);
        });
    }
}
