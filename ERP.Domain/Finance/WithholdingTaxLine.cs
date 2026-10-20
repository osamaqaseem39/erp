using ERP.Domain;

namespace ERP.Domain.Finance;

public class WithholdingTaxLine : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid TaxCodeId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public Guid? PayableAccountId { get; set; }

    public Payment Payment { get; set; } = null!;
    public TaxCode TaxCode { get; set; } = null!;
}
