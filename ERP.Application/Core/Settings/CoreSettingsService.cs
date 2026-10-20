using ERP.Domain.Admin;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Application.Core.Settings;

public interface ICoreSettingsService
{
    Task<SettingsCategoryDto> GetCategoryAsync(string category);
    Task<SettingsCategoryDto> UpdateCategoryAsync(string category, UpdateSettingsCategoryDto dto);
    /// <summary>Server-side read of a single value (decrypted for secrets), falling back to the catalog default.</summary>
    Task<string?> GetValueAsync(string key);
    /// <summary>Server-side write of a single catalog key (e.g. last-used voucher defaults).</summary>
    Task SetValueAsync(string key, string? value);
}

/// <summary>Typed tenant settings backed by the SystemSettings table and validated against <see cref="SettingsCatalog"/>.</summary>
public class CoreSettingsService : ICoreSettingsService
{
    private readonly ISystemSettingRepository _repository;
    private readonly ISecretProtector _protector;

    public CoreSettingsService(ISystemSettingRepository repository, ISecretProtector protector)
    {
        _repository = repository;
        _protector = protector;
    }

    public async Task<SettingsCategoryDto> GetCategoryAsync(string category)
    {
        var definitions = Definitions(category);
        var stored = (await _repository.GetByCategoryAsync(CategoryKey(category))).ToDictionary(s => s.SettingKey, StringComparer.OrdinalIgnoreCase);
        return new SettingsCategoryDto
        {
            Category = category.ToLowerInvariant(),
            Settings = definitions.Select(d => ToDto(d, stored.GetValueOrDefault(d.Key))).ToList(),
        };
    }

    public async Task<SettingsCategoryDto> UpdateCategoryAsync(string category, UpdateSettingsCategoryDto dto)
    {
        var definitions = Definitions(category).ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var key in dto.Values.Keys.Where(k => !definitions.ContainsKey(k)))
            errors.Add($"'{key}' is not a setting in {category}.");
        foreach (var (key, value) in dto.Values.Where(kv => definitions.ContainsKey(kv.Key)))
        {
            var definition = definitions[key];
            if (definition.IsSecret && value is null) continue; // keep the stored secret
            if (definition.ReadOnly) throw new ConflictException($"{definition.Label} is read-only.");
            var error = definition.Validate(value?.Trim());
            if (error != null) errors.Add($"{key}: {error}");
        }
        if (errors.Count > 0) throw new ValidationException("Some settings are not valid.", errors);

        var stored = (await _repository.GetByCategoryAsync(CategoryKey(category))).ToDictionary(s => s.SettingKey, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, raw) in dto.Values)
        {
            var definition = definitions[key];
            if (definition.IsSecret && raw is null) continue;
            var value = raw?.Trim() ?? string.Empty;
            var toStore = definition.IsSecret && value.Length > 0 ? _protector.Protect(value) : value;

            if (stored.TryGetValue(definition.Key, out var existing))
            {
                if (existing.IsReadOnly) throw new ConflictException($"{definition.Label} is read-only.");
                existing.SettingValue = toStore;
                existing.IsEncrypted = definition.IsSecret;
                existing.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(existing);
            }
            else
            {
                await _repository.CreateAsync(new SystemSetting
                {
                    SettingKey = definition.Key,
                    SettingValue = toStore,
                    Category = CategoryKey(category),
                    SettingType = definition.Type.ToString(),
                    Description = definition.Label,
                    IsEncrypted = definition.IsSecret,
                    IsReadOnly = definition.ReadOnly,
                });
            }
        }

        return await GetCategoryAsync(category);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var definition = SettingsCatalog.Find(key) ?? throw new KeyNotFoundException($"Unknown setting '{key}'.");
        var stored = await _repository.GetByKeyAsync(definition.Key);
        if (stored is null || string.IsNullOrEmpty(stored.SettingValue)) return definition.IsSecret ? null : definition.Default;
        return stored.IsEncrypted ? _protector.Unprotect(stored.SettingValue) : stored.SettingValue;
    }

    public async Task SetValueAsync(string key, string? value)
    {
        var definition = SettingsCatalog.Find(key) ?? throw new KeyNotFoundException($"Unknown setting '{key}'.");
        if (definition.ReadOnly) throw new ConflictException($"{definition.Label} is read-only.");

        var trimmed = value?.Trim() ?? string.Empty;
        var error = definition.Validate(string.IsNullOrEmpty(trimmed) ? null : trimmed);
        if (error != null) throw new ValidationException("Some settings are not valid.", [error]);

        var toStore = definition.IsSecret && trimmed.Length > 0 ? _protector.Protect(trimmed) : trimmed;
        var existing = await _repository.GetByKeyAsync(definition.Key);
        if (existing is not null)
        {
            if (existing.IsReadOnly) throw new ConflictException($"{definition.Label} is read-only.");
            existing.SettingValue = toStore;
            existing.IsEncrypted = definition.IsSecret;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
            return;
        }

        await _repository.CreateAsync(new SystemSetting
        {
            SettingKey = definition.Key,
            SettingValue = toStore,
            Category = CategoryKey(definition.Category),
            SettingType = definition.Type.ToString(),
            Description = definition.Label,
            IsEncrypted = definition.IsSecret,
            IsReadOnly = definition.ReadOnly,
        });
    }

    private static IReadOnlyList<SettingDefinition> Definitions(string category)
    {
        var definitions = SettingsCatalog.InCategory(category).ToList();
        if (definitions.Count == 0) throw new KeyNotFoundException($"There is no settings category '{category}'.");
        return definitions;
    }

    private static string CategoryKey(string category) => category.ToLowerInvariant();

    private static SettingValueDto ToDto(SettingDefinition d, SystemSetting? stored)
    {
        var hasStoredValue = stored is not null && !string.IsNullOrEmpty(stored.SettingValue);
        return new SettingValueDto
        {
            Key = d.Key,
            Label = d.Label,
            Type = d.Type.ToString().ToLowerInvariant(),
            // Secrets are write-only: the API reports whether one is set, never what it is.
            Value = d.IsSecret ? null : hasStoredValue ? stored!.SettingValue : d.Default,
            HasValue = hasStoredValue,
            IsDefault = !hasStoredValue,
            Description = d.Description,
            Required = d.Required,
            ReadOnly = d.ReadOnly || stored?.IsReadOnly == true,
            Min = d.Min,
            Max = d.Max,
            Options = d.Options?.Select(o => new SettingOptionDto { Value = o.Value, Label = o.Label }).ToList(),
        };
    }
}
