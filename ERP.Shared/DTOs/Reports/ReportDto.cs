using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Reports;

public class ReportDto : BaseDto
{
    public Guid? Id { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
}

public class CreateReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateReportDto
{
    public new Guid Id { get; set; }
    public string? ReportName { get; set; }
    public string? Name { get; set; }
    public string? ReportCode { get; set; }
    public string? Description { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
}