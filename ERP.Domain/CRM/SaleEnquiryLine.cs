namespace ERP.Domain.CRM;

public class SaleEnquiryLine : PackagingLineFields
{
    public Guid SaleEnquiryId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }

    public SaleEnquiry SaleEnquiry { get; set; } = null!;
}
