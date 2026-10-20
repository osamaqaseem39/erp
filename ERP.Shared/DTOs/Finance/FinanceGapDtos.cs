namespace ERP.Shared.DTOs.Finance;

public class BudgetLineDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }
}

public class CreateBudgetLineDto
{
    public Guid BudgetId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }
}

public class TaxGroupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxType { get; set; } = "Sales";
    public List<TaxGroupLineDto> Lines { get; set; } = new();
}

public class TaxGroupLineDto
{
    public Guid Id { get; set; }
    public Guid TaxCodeId { get; set; }
    public int Sequence { get; set; }
    public bool IsCompound { get; set; }
}

public class CreateTaxGroupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxType { get; set; } = "Sales";
    public List<CreateTaxGroupLineDto> Lines { get; set; } = new();
}

public class CreateTaxGroupLineDto
{
    public Guid TaxCodeId { get; set; }
    public int Sequence { get; set; } = 1;
    public bool IsCompound { get; set; }
}

public class AssetCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsefulLifeMonths { get; set; }
    public string DepreciationMethod { get; set; } = "StraightLine";
    public Guid? AssetAccountId { get; set; }
    public Guid? AccumDepAccountId { get; set; }
    public Guid? DepExpenseAccountId { get; set; }
}

public class CreateAssetCategoryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsefulLifeMonths { get; set; } = 60;
    public string DepreciationMethod { get; set; } = "StraightLine";
    public Guid? AssetAccountId { get; set; }
    public Guid? AccumDepAccountId { get; set; }
    public Guid? DepExpenseAccountId { get; set; }
}

public class AssetDisposalDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateTime DisposalDate { get; set; }
    public decimal Proceeds { get; set; }
    public decimal BookValue { get; set; }
    public decimal GainLoss { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }
}

public class CreateAssetDisposalDto
{
    public Guid AssetId { get; set; }
    public DateTime DisposalDate { get; set; }
    public decimal Proceeds { get; set; }
    public string? Notes { get; set; }
}

public class AssetTransferDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? FromCostCenterId { get; set; }
    public Guid? ToCostCenterId { get; set; }
    public Guid? FromBranchId { get; set; }
    public Guid? ToBranchId { get; set; }
    public string? Notes { get; set; }
}

public class CreateAssetTransferDto
{
    public Guid AssetId { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? FromCostCenterId { get; set; }
    public Guid? ToCostCenterId { get; set; }
    public Guid? FromBranchId { get; set; }
    public Guid? ToBranchId { get; set; }
    public string? Notes { get; set; }
}

public class FundTransferDto
{
    public Guid Id { get; set; }
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

public class CreateFundTransferDto
{
    public string? TransferNumber { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? FromBankAccountId { get; set; }
    public Guid? ToBankAccountId { get; set; }
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
}

public class CreditMemoDto
{
    public Guid Id { get; set; }
    public string MemoNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime MemoDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal SubTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }
    public List<MemoLineDto> Lines { get; set; } = new();
}

public class DebitMemoDto
{
    public Guid Id { get; set; }
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
    public List<MemoLineDto> Lines { get; set; } = new();
}

public class MemoLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid? AccountId { get; set; }
}

public class CreateMemoLineDto
{
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public Guid? AccountId { get; set; }
}

public class CreateCreditMemoDto
{
    public string? MemoNumber { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime MemoDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public List<CreateMemoLineDto> Lines { get; set; } = new();
}

public class CreateDebitMemoDto
{
    public string? MemoNumber { get; set; }
    public Guid VendorId { get; set; }
    public DateTime MemoDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public List<CreateMemoLineDto> Lines { get; set; } = new();
}

public class CustomerAdvanceDto
{
    public Guid Id { get; set; }
    public string AdvanceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}

public class VendorAdvanceDto
{
    public Guid Id { get; set; }
    public string AdvanceNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
}

public class CreateCustomerAdvanceDto
{
    public string? AdvanceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
}

public class CreateVendorAdvanceDto
{
    public string? AdvanceNumber { get; set; }
    public Guid VendorId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
}

public class CreateAdvanceApplicationDto
{
    public Guid? CustomerAdvanceId { get; set; }
    public Guid? VendorAdvanceId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AppliedAmount { get; set; }
}

public class AdvanceApplicationDto
{
    public Guid Id { get; set; }
    public Guid? CustomerAdvanceId { get; set; }
    public Guid? VendorAdvanceId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Applied";
}

public class WithholdingTaxLineDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid TaxCodeId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public Guid? PayableAccountId { get; set; }
}

public class CreateWithholdingTaxLineDto
{
    public Guid PaymentId { get; set; }
    public Guid TaxCodeId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal WithholdingAmount { get; set; }
    public Guid? PayableAccountId { get; set; }
}

public class RecurringJournalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JournalType { get; set; } = "Journal";
    public string Frequency { get; set; } = "Monthly";
    public DateTime NextRunDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public List<RecurringJournalLineDto> Lines { get; set; } = new();
}

public class RecurringJournalLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
}

public class CreateRecurringJournalDto
{
    public string Name { get; set; } = string.Empty;
    public string JournalType { get; set; } = "Journal";
    public string Frequency { get; set; } = "Monthly";
    public DateTime NextRunDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<CreateRecurringJournalLineDto> Lines { get; set; } = new();
}

public class CreateRecurringJournalLineDto
{
    public Guid AccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
}

public class FxRevaluationRunDto
{
    public Guid Id { get; set; }
    public DateTime RunDate { get; set; }
    public string? RateSource { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }
    public List<FxRevaluationLineDto> Lines { get; set; } = new();
}

public class FxRevaluationLineDto
{
    public Guid Id { get; set; }
    public Guid? AccountId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ForeignBalance { get; set; }
    public decimal BookBalance { get; set; }
    public decimal RevaluedBalance { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
}

public class CreateFxRevaluationRunDto
{
    public DateTime RunDate { get; set; }
    public string? RateSource { get; set; }
    public string? Notes { get; set; }
    public List<CreateFxRevaluationLineDto> Lines { get; set; } = new();
}

public class CreateFxRevaluationLineDto
{
    public Guid? AccountId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ForeignBalance { get; set; }
    public decimal BookBalance { get; set; }
    public decimal RevaluedBalance { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
}

public class IntercompanySettlementDto
{
    public Guid Id { get; set; }
    public string SettlementNumber { get; set; } = string.Empty;
    public DateTime SettlementDate { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }
    public List<Guid> TransactionIds { get; set; } = new();
}

public class CreateIntercompanySettlementDto
{
    public string? SettlementNumber { get; set; }
    public DateTime SettlementDate { get; set; }
    public string? Notes { get; set; }
    public List<Guid> TransactionIds { get; set; } = new();
}

public class IntercompanyTransactionDto
{
    public Guid Id { get; set; }
    public Guid FromCompanyId { get; set; }
    public Guid ToCompanyId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid? SettlementId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateIntercompanyTransactionDto
{
    public Guid FromCompanyId { get; set; }
    public Guid ToCompanyId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
}
