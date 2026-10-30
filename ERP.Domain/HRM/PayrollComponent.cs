using ERP.Domain;

namespace ERP.Domain.HRM;

public class PayrollComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Type { get; set; } = string.Empty; // Basic/Earning/Deduction/Tax
    public string CalculationType { get; set; } = string.Empty; // Fixed, PercentOfBasic, PercentOfGross
    public decimal Value { get; set; }
    public bool IsTaxable { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}
