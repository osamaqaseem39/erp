using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class JournalDto : BaseDto
{
    public Guid? JournalId { get; set; }
    public new Guid? TenantId { get; set; }
    public string? JournalType { get; set; }
    public string? JournalNumber { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? JournalDate { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public string? Status { get; set; }
    public string? ChequeNumber { get; set; }
    public string? ChequeStatus { get; set; }
    public DateTime? ChequeStatusAt { get; set; }
    public string? ChequeStatusNotes { get; set; }
    public string? PaidBy { get; set; }
    public string? ReceivedBy { get; set; }
    public Guid? CashOrBankAccountId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
    public new DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public bool? IsLocked { get; set; }
    public Guid? FinancialYearId { get; set; }
    public List<JournalEntryDto>? JournalEntries { get; set; }
}


public class JournalEntryDto : BaseDto
{
    public Guid EntryId { get; set; }
    public Guid JournalId { get; set; }
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
}

public class CreateJournalDto : CreateDto
{
    public string JournalType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public DateTime JournalDate { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? ChequeNumber { get; set; }
    public string? PaidBy { get; set; }
    public string? ReceivedBy { get; set; }
    public Guid? CashOrBankAccountId { get; set; }
    /// <summary>Optional invoice/bill number on the voucher header.</summary>
    public string? InvoiceNumber { get; set; }
    /// <summary>Optional link table (e.g. Invoices, Bills).</summary>
    public string? EntityReferenceTable { get; set; }
    /// <summary>Optional linked AR invoice or AP bill id.</summary>
    public Guid? EntityReferenceId { get; set; }
    /// <summary>Ignored on create — always Draft. Use approve/post endpoints.</summary>
    public string Status { get; set; } = "Draft";
    public bool IsLocked { get; set; } = false;
    /// <summary>
    /// Offset lines for typed cash/bank vouchers.
    /// CashPayment/BankPayment: lines are debited; CashOrBankAccountId is credited.
    /// CashReceipt/BankReceipt: lines are credited; CashOrBankAccountId is debited.
    /// </summary>
    public List<CreateVoucherLineDto>? OffsetLines { get; set; }
    /// <summary>Alias for OffsetLines (CashPayment / BankPayment).</summary>
    public List<CreateVoucherLineDto>? DebitLines { get; set; }
    /// <summary>Alias for OffsetLines (CashReceipt / BankReceipt).</summary>
    public List<CreateVoucherLineDto>? CreditLines { get; set; }
}

public class CreateVoucherLineDto
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Narration { get; set; }
    public Guid? CostCenterId { get; set; }
}

/// <summary>Backward-compatible alias.</summary>
public class CreateCashPaymentLineDto : CreateVoucherLineDto { }

public class UpdateJournalDto : UpdateDto
{
    public Guid? JournalId { get; set; }
    public string? JournalType { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? JournalDate { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? ChequeNumber { get; set; }
    public string? PaidBy { get; set; }
    public string? ReceivedBy { get; set; }
    public Guid? CashOrBankAccountId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
    /// <summary>Ignored — status only via approve/post/reject/reverse.</summary>
    public string? Status { get; set; }
    public bool? IsLocked { get; set; }
}

public class CreateJournalEntryDto : CreateDto
{
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
}

public class UpdateJournalEntryDto : UpdateDto
{
    public Guid? EntryId { get; set; }
    public Guid? AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public string? Narration { get; set; }
    public string? EntityReferenceTable { get; set; }
    public Guid? EntityReferenceId { get; set; }
}


public class JournalQuery
{
    public Guid TenantId { get; set; }
    public string? JournalType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// Aliases for interface compatibility
public class JournalCreateDto : CreateJournalDto { }
public class JournalUpdateDto : UpdateJournalDto { }