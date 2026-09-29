namespace ERP.Shared.DTOs.Core;

public class SettingOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class SettingValueDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    /// <summary>text, number, boolean, choice, email or secret.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Current value (or the default). Always null for secrets.</summary>
    public string? Value { get; set; }
    /// <summary>For secrets: whether a value is stored. For others: whether the value differs from the default.</summary>
    public bool HasValue { get; set; }
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public bool Required { get; set; }
    public bool ReadOnly { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public List<SettingOptionDto>? Options { get; set; }
}

public class SettingsCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public List<SettingValueDto> Settings { get; set; } = new();
}

/// <summary>
/// Values to change, keyed by setting key. Omitted keys are left as they are. For secrets,
/// null/omitted keeps the stored value and "" clears it.
/// </summary>
public class UpdateSettingsCategoryDto
{
    public Dictionary<string, string?> Values { get; set; } = new();
}
