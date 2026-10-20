using ERP.Domain;

namespace ERP.Domain.Finance;

public class ChequeBook : BaseEntity
{
    public Guid BankAccountId { get; set; }
    public string BookNumber { get; set; } = string.Empty;
    public int StartLeafNumber { get; set; }
    public int EndLeafNumber { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<ChequeLeaf> Leaves { get; set; } = new List<ChequeLeaf>();
}

public class ChequeLeaf : BaseEntity
{
    public Guid ChequeBookId { get; set; }
    public int LeafNumber { get; set; }
    public string Status { get; set; } = "Blank";
    public DateTime? IssuedAt { get; set; }
    public Guid? JournalId { get; set; }
    public string? Notes { get; set; }

    public ChequeBook ChequeBook { get; set; } = null!;
}
