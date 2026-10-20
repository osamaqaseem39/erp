using ERP.Domain;

namespace ERP.Domain.HRM;

public class EmployeeBenefitEnrollment : BaseEntity
{
    public Guid BenefitId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? UnenrollmentDate { get; set; }
    /// <summary>Active / Ended</summary>
    public string Status { get; set; } = "Active";
    public string? Reason { get; set; }

    public Benefit Benefit { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
