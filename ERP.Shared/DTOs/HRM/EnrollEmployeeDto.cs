using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EnrollEmployeeDto
{
    public Guid BenefitId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = "Active";
}