using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class PayrollDto : BaseDto
{
    public string PayrollNumber { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string Currency { get; set; } = "USD";
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
}

public class PayrollLineDto : BaseDto
{
    public Guid PayrollId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Earnings { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPay { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
}

public class PayrollRunDto : BaseDto
{
    public Guid PayrollId { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public DateTime RunDate { get; set; }
    public string Status { get; set; } = "Running";
    public int TotalEmployees { get; set; }
    public int ProcessedEmployees { get; set; }
    public int FailedEmployees { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
}

public class PayrollResultDto : BaseDto
{
    public Guid PayrollId { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public int EmployeeCount { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalNetPay { get; set; }
    public List<PayrollLineDto> PayrollLines { get; set; } = new List<PayrollLineDto>();
}

public class CreatePayrollDto : CreateDto
{
    public string? PayrollNumber { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
}

public class UpdatePayrollDto : UpdateDto
{
    public string? PayrollNumber { get; set; }
    public DateTime? PayPeriodStart { get; set; }
    public DateTime? PayPeriodEnd { get; set; }
    public DateTime? PayDate { get; set; }
    public string? Status { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
}