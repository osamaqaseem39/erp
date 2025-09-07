using ERP.Domain;

namespace ERP.Domain.Reports;

public class Report : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReportType { get; set; } = string.Empty; // Financial, Operational, Analytical
    public string Category { get; set; } = string.Empty; // Sales, Purchase, Inventory, HR, Finance
    public string Query { get; set; } = string.Empty; // SQL Query or stored procedure name
    public string Parameters { get; set; } = string.Empty; // JSON string of parameters
    public string OutputFormat { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<ReportSchedule> ReportSchedules { get; set; } = new List<ReportSchedule>();
    public ICollection<ReportExecution> ReportExecutions { get; set; } = new List<ReportExecution>();
}