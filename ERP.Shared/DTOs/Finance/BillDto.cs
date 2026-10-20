using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance
{
    public class BillDto : BaseDto
    {
        public Guid CompanyId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public new bool IsActive { get; set; }
    }

    public class CreateBillDto : CreateDto
    {
        public Guid CompanyId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public string Notes { get; set; } = string.Empty;
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Reference { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public Guid? PurchaseInvoiceId { get; set; }
        public new bool IsActive { get; set; } = true;
    }

    public class UpdateBillDto : UpdateDto
    {
        public Guid BillId { get; set; }
        public string? BillNumber { get; set; }
        public Guid? VendorId { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Reference { get; set; }
        public bool? IsActive { get; set; }
    }
}