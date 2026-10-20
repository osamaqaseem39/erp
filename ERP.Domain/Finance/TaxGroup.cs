using ERP.Domain;

namespace ERP.Domain.Finance;

public class TaxGroup : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxType { get; set; } = "Sales";

    public ICollection<TaxGroupLine> Lines { get; set; } = new List<TaxGroupLine>();
}

public class TaxGroupLine : BaseEntity
{
    public Guid TaxGroupId { get; set; }
    public Guid TaxCodeId { get; set; }
    public int Sequence { get; set; } = 1;
    public bool IsCompound { get; set; }

    public TaxGroup TaxGroup { get; set; } = null!;
    public TaxCode TaxCode { get; set; } = null!;
}
