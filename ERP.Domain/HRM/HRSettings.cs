using ERP.Domain;

namespace ERP.Domain.HRM;

public class HRSettings : BaseEntity
{
    public string DefaultPayFrequency { get; set; } = "Monthly";
    public string? WorkingDaysJson { get; set; } // Mon-Fri config
    public string? OvertimePolicyJson { get; set; }
    public string? LeaveAccrualPolicyJson { get; set; }
    public string? TaxSettingsJson { get; set; }
    public string? ComplianceSettingsJson { get; set; }
}
