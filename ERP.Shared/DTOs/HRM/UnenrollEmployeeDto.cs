using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class UnenrollEmployeeDto
{
    public Guid BenefitId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime UnenrollmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}