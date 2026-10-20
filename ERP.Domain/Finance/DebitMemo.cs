using ERP.Domain;

namespace ERP.Domain.Finance;

public class DebitMemo : BaseEntity
{
    public string MemoNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public DateTime MemoDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }

    public ICollection<DebitMemoLine> Lines { get; set; } = new List<DebitMemoLine>();
}

public class DebitMemoLine : BaseEntity
{
    public Guid DebitMemoId { get; set; }
    public int LineNumber { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid? AccountId { get; set; }

    public DebitMemo DebitMemo { get; set; } = null!;
}
