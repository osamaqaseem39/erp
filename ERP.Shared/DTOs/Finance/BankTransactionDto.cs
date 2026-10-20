using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class BankTransactionDto : BaseDto
{
    public Guid BankAccountId { get; set; }
    public string? BankReference { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ImportedAt { get; set; }
    public Guid? MatchedPaymentId { get; set; }
    public Guid? ReconciliationId { get; set; }
    public bool IsReconciled { get; set; }
    public string? BankAccountName { get; set; }
    public string? MatchedPaymentNumber { get; set; }
}

public class CreateBankTransactionDto : CreateDto
{
    public Guid BankAccountId { get; set; }
    public string? BankReference { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Description { get; set; }
}

public class UpdateBankTransactionDto : UpdateDto
{
    public string? BankReference { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Description { get; set; }
    public bool? IsReconciled { get; set; }
}

public class BankTransactionMatchDto : BaseDto
{
    public Guid BankTransactionId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal MatchAmount { get; set; }
    public string? Notes { get; set; }
}

public class BankTransactionCreateDto : CreateBankTransactionDto { }