using ERP.Domain;

namespace ERP.Domain.Core;

public class Setting : BaseEntity
{
    public new Guid TenantId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SettingType { get; set; } = string.Empty; // String, Number, Boolean, JSON
    public string Category { get; set; } = string.Empty; // General, Security, Email, Integration
    public bool IsEncrypted { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
}