using ERP.Domain;

namespace ERP.Domain.HRM;

public class PayrollLine : BaseEntity
{
    public Guid PayrollId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal Earnings { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPay { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? PayrollComponentId { get; set; }
    
    // Navigation properties
    public Payroll Payroll { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public PayrollComponent? PayrollComponent { get; set; }
}
