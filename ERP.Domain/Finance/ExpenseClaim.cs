using ERP.Domain;

namespace ERP.Domain.Finance;

public class ExpenseClaim : BaseEntity
{
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime ClaimDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<ExpenseClaimLine> Lines { get; set; } = new List<ExpenseClaimLine>();
}

public class ExpenseClaimLine : BaseEntity
{
    public Guid ExpenseClaimId { get; set; }
    public int LineNumber { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? AccountId { get; set; }

    public ExpenseClaim ExpenseClaim { get; set; } = null!;
}
