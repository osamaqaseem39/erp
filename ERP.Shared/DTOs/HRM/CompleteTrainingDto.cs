using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class CompleteTrainingDto
{
    public Guid TrainingId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CompletionDate { get; set; }
    public int? Score { get; set; }
    public string? Certificate { get; set; }
    public string? Notes { get; set; }
}