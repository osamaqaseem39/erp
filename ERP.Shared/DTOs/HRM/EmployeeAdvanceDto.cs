namespace ERP.Shared.DTOs.HRM;

public class EmployeeAdvanceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal RecoveredAmount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Disbursed";
    public string? Notes { get; set; }
}

public class CreateEmployeeAdvanceDto
{
    public Guid EmployeeId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class RecoverEmployeeAdvanceDto
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class TeamDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ManagerEmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ManagerEmployeeId { get; set; }
}

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public bool? IsActive { get; set; }
}
