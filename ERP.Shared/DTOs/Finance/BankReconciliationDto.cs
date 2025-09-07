using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class BankReconciliationDto : BaseDto
{
    public Guid BankAccountId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public string? BankAccountName { get; set; }
}

public class CreateBankReconciliationDto : CreateDto
{
    public Guid BankAccountId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class UpdateBankReconciliationDto : UpdateDto
{
    public DateTime? ReconciliationDate { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public string? Status { get; set; }
}

public class BankReconciliationCreateDto : CreateBankReconciliationDto { }