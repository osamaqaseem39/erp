namespace ERP.Shared.DTOs.Finance;

public class OpeningBalanceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FiscalYearId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime AsOfDate { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? JournalId { get; set; }
    public string? Notes { get; set; }
    public List<OpeningBalanceLineDto> Lines { get; set; } = new();
}

public class OpeningBalanceLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string? Narration { get; set; }
}

public class CreateOpeningBalanceDto
{
    public Guid FiscalYearId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime AsOfDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateOpeningBalanceLineDto> Lines { get; set; } = new();
}

public class CreateOpeningBalanceLineDto
{
    public int LineNumber { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string? Narration { get; set; }
}

public class AccountingPeriodDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Open";
}

public class CreateAccountingPeriodDto
{
    public Guid FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ChequeBookDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BankAccountId { get; set; }
    public string BookNumber { get; set; } = string.Empty;
    public int StartLeafNumber { get; set; }
    public int EndLeafNumber { get; set; }
    public string Status { get; set; } = "Active";
    public List<ChequeLeafDto> Leaves { get; set; } = new();
}

public class ChequeLeafDto
{
    public Guid Id { get; set; }
    public int LeafNumber { get; set; }
    public string Status { get; set; } = "Blank";
    public DateTime? IssuedAt { get; set; }
    public Guid? JournalId { get; set; }
    public string? Notes { get; set; }
}

public class CreateChequeBookDto
{
    public Guid BankAccountId { get; set; }
    public string BookNumber { get; set; } = string.Empty;
    public int StartLeafNumber { get; set; }
    public int EndLeafNumber { get; set; }
}

public class IssueChequeLeafDto
{
    public Guid? JournalId { get; set; }
    public string? Notes { get; set; }
}

public class SetChequeLeafStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ExpenseClaimDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime ClaimDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public List<ExpenseClaimLineDto> Lines { get; set; } = new();
}

public class ExpenseClaimLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? AccountId { get; set; }
}

public class CreateExpenseClaimDto
{
    public string? ClaimNumber { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime ClaimDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public List<CreateExpenseClaimLineDto> Lines { get; set; } = new();
}

public class CreateExpenseClaimLineDto
{
    public int LineNumber { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? AccountId { get; set; }
}
