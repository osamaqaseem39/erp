using ERP.Shared.DTOs.Procurement;
using FluentValidation;

namespace ERP.Api.Validators.Procurement;

public class UpdatePurchaseDebitNoteValidator : BaseValidator<UpdatePurchaseDebitNoteDto>
{
    public UpdatePurchaseDebitNoteValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.DebitNoteNumber)
            .NotEmpty().WithMessage("Debit note number is required")
            .MaximumLength(50);

        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.OriginalPurchaseInvoiceId)
            .NotEmpty().WithMessage("Original purchase invoice ID is required");

        RuleFor(x => x.DebitNoteDate)
            .NotEmpty().WithMessage("Debit note date is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one debit note line is required");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.TaxRate).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Description).MaximumLength(500);
        });
    }
}
