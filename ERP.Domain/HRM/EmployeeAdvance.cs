using ERP.Domain;

namespace ERP.Domain.HRM;

public class EmployeeAdvance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal RecoveredAmount { get; set; }
    public string Status { get; set; } = "Disbursed";
    public string? Notes { get; set; }
    public DateTime? DisbursedAt { get; set; }

    public Employee? Employee { get; set; }
}

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? ManagerEmployeeId { get; set; }

    public Employee? Manager { get; set; }
}
