using ERP.Application.Core.Settings;
using ERP.Shared.DTOs.Core;

namespace ERP.Tests;

/// <summary>Default-off tenant settings for unit tests.</summary>
public sealed class TestCoreSettings : ICoreSettingsService
{
    private readonly Dictionary<string, string?> _values = new(StringComparer.OrdinalIgnoreCase);

    public Task<SettingsCategoryDto> GetCategoryAsync(string category) =>
        throw new NotImplementedException();

    public Task<SettingsCategoryDto> UpdateCategoryAsync(string category, UpdateSettingsCategoryDto dto) =>
        throw new NotImplementedException();

    public Task<string?> GetValueAsync(string key) =>
        Task.FromResult(_values.GetValueOrDefault(key));

    public Task SetValueAsync(string key, string? value)
    {
        _values[key] = value;
        return Task.CompletedTask;
    }
}
