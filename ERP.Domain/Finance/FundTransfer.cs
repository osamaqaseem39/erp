using ERP.Domain;

namespace ERP.Domain.Finance;

public class FundTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public Guid? FromBankAccountId { get; set; }
    public Guid? ToBankAccountId { get; set; }
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }
}
