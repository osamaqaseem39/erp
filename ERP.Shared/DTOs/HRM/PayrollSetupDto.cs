namespace ERP.Shared.DTOs.HRM;

public class PayrollComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    /// <summary>Basic, Earning, Deduction or Tax.</summary>
    public string Type { get; set; } = "Earning";
    /// <summary>Fixed, PercentOfBasic or PercentOfGross.</summary>
    public string CalculationType { get; set; } = "Fixed";
    public decimal Value { get; set; }
    public bool IsTaxable { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class EmployeeSalaryComponentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public Guid PayrollComponentId { get; set; }
    public string? ComponentName { get; set; }
    public string? ComponentType { get; set; }
    public string? CalculationType { get; set; }
    /// <summary>Override of the component default; null uses the component value.</summary>
    public decimal? Value { get; set; }
    public decimal EffectiveValue { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PayslipItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PayslipDto
{
    public Guid LineId { get; set; }
    public Guid PayrollId { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPay { get; set; }
    public List<PayslipItemDto> Earnings { get; set; } = new();
    public List<PayslipItemDto> DeductionItems { get; set; } = new();
}
