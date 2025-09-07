using ERP.Domain;

namespace ERP.Domain.Admin;

public class SystemSetting : BaseEntity
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SettingType { get; set; } = string.Empty; // String, Number, Boolean, JSON
    public string Category { get; set; } = string.Empty; // General, Security, Email, Integration
    public bool IsEncrypted { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
}