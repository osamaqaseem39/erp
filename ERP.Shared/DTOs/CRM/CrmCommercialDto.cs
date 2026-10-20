namespace ERP.Shared.DTOs.CRM;

public class CrmCommercialHeaderFields
{
    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public int CreditDays { get; set; }
    public string? PaymentMode { get; set; }
    public Guid? PaymentTermId { get; set; }
    public decimal Freight { get; set; }
    public decimal Overhead { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
}
