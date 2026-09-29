using ERP.Application.Admin;
using ERP.Application.Core.Settings;
using ERP.Domain.Admin;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class SettingsTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly ERPDbContext _db = new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly FakeProtector _protector = new();
    private readonly CoreSettingsService _settings;
    private readonly SystemSettingService _raw;

    public SettingsTests()
    {
        var repository = new SystemSettingRepository(_db, new FixedTenant(Tenant));
        _settings = new CoreSettingsService(repository, _protector);
        _raw = new SystemSettingService(repository, _protector);
    }

    [Fact]
    public async Task UnsetCategory_ReturnsCatalogDefaults()
    {
        var prefs = await _settings.GetCategoryAsync("preferences");

        var locale = prefs.Settings.Single(s => s.Key == "preferences.locale");
        Assert.Equal(("en-US", true, "choice"), (locale.Value, locale.IsDefault, locale.Type));
        Assert.NotEmpty(locale.Options!);
    }

    [Fact]
    public async Task ValidValuesAreStored_AndReturned()
    {
        await _settings.UpdateCategoryAsync("security", Values(("security.sessionTimeoutMinutes", "120")));

        var security = await _settings.GetCategoryAsync("security");
        Assert.Equal("120", security.Settings.Single(s => s.Key == "security.sessionTimeoutMinutes").Value);
        Assert.Equal("120", await _settings.GetValueAsync("security.sessionTimeoutMinutes"));
    }

    [Theory]
    [InlineData("security.sessionTimeoutMinutes", "2")]
    [InlineData("security.sessionTimeoutMinutes", "abc")]
    [InlineData("preferences.locale", "xx-XX")]
    public async Task InvalidValuesAreRejected(string key, string value)
    {
        var category = key.Split('.')[0];
        await Assert.ThrowsAsync<ValidationException>(() => _settings.UpdateCategoryAsync(category, Values((key, value))));
    }

    [Fact]
    public async Task UnknownKeysAndCategoriesAreRejected()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _settings.UpdateCategoryAsync("email", Values(("email.madeUp", "x"))));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _settings.GetCategoryAsync("nope"));
    }

    [Fact]
    public async Task Secrets_AreEncryptedAtRest_NeverReturned_AndKeptWhenOmitted()
    {
        await _settings.UpdateCategoryAsync("email", Values(("email.smtpPassword", "s3cret!")));

        var row = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "email.smtpPassword");
        Assert.True(row.IsEncrypted);
        Assert.NotEqual("s3cret!", row.SettingValue);

        var dto = (await _settings.GetCategoryAsync("email")).Settings.Single(s => s.Key == "email.smtpPassword");
        Assert.Null(dto.Value);
        Assert.True(dto.HasValue);

        // Saving the form without touching the password keeps it.
        await _settings.UpdateCategoryAsync("email", Values(("email.smtpHost", "smtp.x.test"), ("email.smtpPassword", null)));
        Assert.Equal("s3cret!", await _settings.GetValueAsync("email.smtpPassword"));

        // An empty string clears it.
        await _settings.UpdateCategoryAsync("email", Values(("email.smtpPassword", "")));
        Assert.Null(await _settings.GetValueAsync("email.smtpPassword"));
    }

    [Fact]
    public async Task RawApi_NeverReturnsEncryptedValues_AndEncryptsOnCreate()
    {
        var created = await _raw.CreateAsync(new CreateSystemSettingDto { SettingKey = "integration.apiKey", SettingValue = "abc", Category = "integration", SettingType = "String", IsEncrypted = true }, Tenant, Guid.NewGuid());

        Assert.Equal(string.Empty, created.SettingValue);
        Assert.NotEqual("abc", (await _db.SystemSettings.SingleAsync(s => s.SettingKey == "integration.apiKey")).SettingValue);
        Assert.All(await _raw.GetAllAsync(Tenant), s => Assert.True(!s.IsEncrypted || s.SettingValue == string.Empty));
    }

    [Fact]
    public async Task ReadOnlySettings_CannotBeChangedOrDeleted()
    {
        var locked = new SystemSetting { TenantId = Tenant, SettingKey = "platform.plan", SettingValue = "pro", Category = "platform", IsReadOnly = true };
        _db.SystemSettings.Add(locked);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => _raw.UpdateAsync(new UpdateSystemSettingDto { Id = locked.Id, SettingValue = "free" }, Tenant, Guid.NewGuid()));
        await Assert.ThrowsAsync<ConflictException>(() => _raw.DeleteAsync(locked.Id, Tenant));
    }

    [Fact]
    public async Task RawApi_ValidatesCatalogKeys()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _raw.UpsertAsync(
            new UpsertSystemSettingDto { SettingKey = "security.lockoutThreshold", SettingValue = "1", Category = "security", SettingType = "Number" }, Tenant, Guid.NewGuid()));
    }

    private static UpdateSettingsCategoryDto Values(params (string Key, string? Value)[] values) =>
        new() { Values = values.ToDictionary(v => v.Key, v => v.Value) };

    private sealed class FakeProtector : ISecretProtector
    {
        public string Protect(string plaintext) => "enc:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plaintext));
        public string Unprotect(string protectedValue) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(protectedValue["enc:".Length..]));
    }

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
