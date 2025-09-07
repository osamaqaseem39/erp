using ERP.Shared.DTOs.Finance;

namespace ERP.Api.GraphQL.Finance;

public class ChartOfAccountType
{
    public Guid AccountId { get; set; }
    public Guid TenantId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsPostingAllowed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class JournalType
{
    public Guid JournalId { get; set; }
    public Guid TenantId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public DateTime JournalDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; }
    public List<JournalEntryType> JournalEntries { get; set; } = new();
}

public class JournalEntryType
{
    public Guid EntryId { get; set; }
    public Guid JournalId { get; set; }
    public Guid TenantId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class InvoiceType
{
    public Guid InvoiceId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; }
    public List<InvoiceLineType> InvoiceLines { get; set; } = new();
}

public class InvoiceLineType
{
    public Guid InvoiceLineId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxRate { get; set; }
    public Guid GLAccountId { get; set; }
    public string GLAccountCode { get; set; } = string.Empty;
    public string GLAccountName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class PaymentType
{
    public Guid PaymentId { get; set; }
    public Guid TenantId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public Guid? RelatedInvoiceId { get; set; }
    public string? RelatedInvoiceNumber { get; set; }
    public Guid? RelatedBillId { get; set; }
    public string? RelatedBillNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
}

public class BankAccountType
{
    public Guid BankAccountId { get; set; }
    public Guid TenantId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string? SWIFT { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public Guid GLAccountId { get; set; }
    public string GLAccountCode { get; set; } = string.Empty;
    public string GLAccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class FixedAssetType
{
    public Guid AssetId { get; set; }
    public Guid TenantId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal AcquisitionCost { get; set; }
    public string DepreciationMethod { get; set; } = string.Empty;
    public int UsefulLifeMonths { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public Guid GLAccountId { get; set; }
    public string GLAccountCode { get; set; } = string.Empty;
    public string GLAccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? DisposedAt { get; set; }
    public Guid? DisposedBy { get; set; }
    public List<DepreciationEntryType> DepreciationEntries { get; set; } = new();
}

public class DepreciationEntryType
{
    public Guid DepreciationId { get; set; }
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal DepreciationAmount { get; set; }
    public Guid? PostedJournalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class TrialBalanceType
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
    public decimal NetBalance { get; set; }
}

public class AgedReceivablesType
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days30 { get; set; }
    public decimal Days60 { get; set; }
    public decimal Days90 { get; set; }
    public decimal Over90 { get; set; }
    public decimal Total { get; set; }
}

public class PagedResultType<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}