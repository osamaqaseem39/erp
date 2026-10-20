namespace ERP.Shared.DTOs.Projects;

public class ProjectTimesheetDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalHours { get; set; }
    public string? Notes { get; set; }
    public List<ProjectTimesheetLineDto> Lines { get; set; } = new();
}

public class ProjectTimesheetLineDto
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public DateTime WorkDate { get; set; }
    public Guid? ProjectTaskId { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    public decimal HourlyRate { get; set; }
}

public class CreateProjectTimesheetDto
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Notes { get; set; }
    public List<CreateProjectTimesheetLineDto> Lines { get; set; } = new();
}

public class CreateProjectTimesheetLineDto
{
    public int LineNumber { get; set; }
    public DateTime WorkDate { get; set; }
    public Guid? ProjectTaskId { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool? IsBillable { get; set; }
    public decimal? HourlyRate { get; set; }
}

public class ProjectCostSummaryDto
{
    public Guid ProjectId { get; set; }
    public decimal Budget { get; set; }
    public decimal LaborCost { get; set; }
    public decimal ExpenseCost { get; set; }
    public decimal GlDebitCost { get; set; }
    public decimal ActualCost { get; set; }
    public decimal StoredActualCost { get; set; }
}

public class MilestoneBillingDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public class MilestoneBillingResultDto
{
    public Guid MilestoneId { get; set; }
    public Guid? FinanceInvoiceId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public string Target { get; set; } = string.Empty;
}
