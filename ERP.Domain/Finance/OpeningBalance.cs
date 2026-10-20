using ERP.Domain;

namespace ERP.Domain.Finance;

public class OpeningBalance : BaseEntity
{
    public Guid FiscalYearId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime AsOfDate { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? JournalId { get; set; }
    public string? Notes { get; set; }

    public ICollection<OpeningBalanceLine> Lines { get; set; } = new List<OpeningBalanceLine>();
}

public class OpeningBalanceLine : BaseEntity
{
    public Guid OpeningBalanceId { get; set; }
    public int LineNumber { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string? Narration { get; set; }

    public OpeningBalance OpeningBalance { get; set; } = null!;
}
