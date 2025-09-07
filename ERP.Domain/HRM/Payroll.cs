using ERP.Domain;

namespace ERP.Domain.HRM;

public class Payroll : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public DateTime PayrollRunDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Posted, Paid
    public DateTime? ProcessedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    
    // Navigation properties
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
