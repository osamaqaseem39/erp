using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class SystemSettingDto : BaseDto
{
    public Guid? Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SettingType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
}

public class CreateSystemSettingDto
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SettingType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSystemSettingDto
{
    public new Guid Id { get; set; }
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SettingType { get; set; }
    public bool? IsEncrypted { get; set; }
    public bool? IsActive { get; set; }
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
}

public class UpsertSystemSettingDto
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SettingType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; } = false;
    public bool? IsActive { get; set; }
    public string? ValidationRule { get; set; }
    public string? Notes { get; set; }
}