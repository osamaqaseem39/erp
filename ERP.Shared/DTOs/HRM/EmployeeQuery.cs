using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EmployeeQuery
{
    public string? SearchTerm { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public Guid? CompanyId { get; set; }
    public Guid? TenantId { get; set; }
    public int Page { get; set; } = 1;
}