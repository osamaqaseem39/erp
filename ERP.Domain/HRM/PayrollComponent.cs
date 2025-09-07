using ERP.Domain;

namespace ERP.Domain.HRM;

public class PayrollComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Earning/Deduction/Tax
    public string CalculationType { get; set; } = string.Empty; // Fixed, Percentage, Formula
    public decimal Value { get; set; }
    public bool IsTaxable { get; set; }
}
