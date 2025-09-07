using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class SettingDto : BaseDto
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string SettingType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSettingDto : CreateDto
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string SettingType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsEncrypted { get; set; } = false;
    public bool? IsActive { get; set; } = true;
}

public class UpdateSettingDto : UpdateDto
{
    public string? SettingValue { get; set; }
    public string? SettingType { get; set; }
    public string? Description { get; set; }
    public bool? IsEncrypted { get; set; }
    public bool? IsActive { get; set; }
}