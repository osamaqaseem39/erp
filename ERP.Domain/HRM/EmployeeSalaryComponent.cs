using ERP.Domain;

namespace ERP.Domain.HRM;

/// <summary>One component of an employee's salary structure (e.g. Basic 80,000; House rent 45% of basic).</summary>
public class EmployeeSalaryComponent : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid PayrollComponentId { get; set; }
    /// <summary>Overrides the component's default value when set (amount for Fixed, percent otherwise).</summary>
    public decimal? Value { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public Employee Employee { get; set; } = null!;
    public PayrollComponent PayrollComponent { get; set; } = null!;
}
