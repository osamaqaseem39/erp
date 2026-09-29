using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

/// <summary>Checkpoint 1 finding F1: settings and logs used to ignore the caller's tenant.</summary>
public class TenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    private readonly ERPDbContext _db = new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private readonly SystemSetting _settingOfB;

    public TenantIsolationTests()
    {
        _settingOfB = new SystemSetting { TenantId = TenantB, SettingKey = "smtp.host", SettingValue = "mail.b.test", Category = "Email", SettingType = "String" };
        _db.SystemSettings.AddRange(
            new SystemSetting { TenantId = TenantA, SettingKey = "smtp.host", SettingValue = "mail.a.test", Category = "Email", SettingType = "String" },
            _settingOfB);
        _db.AuditLogs.AddRange(
            new AuditLog { TenantId = TenantA, EntityName = "Customer", EntityId = "1", Action = "Created" },
            new AuditLog { TenantId = TenantB, EntityName = "Customer", EntityId = "2", Action = "Created" },
            new AuditLog { TenantId = TenantB, EntityName = "Customer", EntityId = "3", Action = "Deleted" });
        _db.SystemLogs.AddRange(
            new SystemLog { TenantId = TenantA, LogLevel = "Error", Message = "A failed" },
            new SystemLog { TenantId = TenantB, LogLevel = "Error", Message = "B failed" });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Settings_OnlyTheCallersTenantIsVisible()
    {
        var repo = new SystemSettingRepository(_db, Tenant(TenantA));

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Single(all);
        Assert.Equal("mail.a.test", all[0].SettingValue);
        Assert.Equal("mail.a.test", (await repo.GetByKeyAsync("smtp.host"))!.SettingValue);
    }

    [Fact]
    public async Task Settings_AnotherTenantsIdIsNotFound_AndCannotBeChangedOrDeleted()
    {
        var repo = new SystemSettingRepository(_db, Tenant(TenantA));

        Assert.Null(await repo.GetByIdAsync(_settingOfB.Id));
        Assert.False(await repo.DeleteAsync(_settingOfB.Id));

        var forged = new SystemSetting { Id = _settingOfB.Id, TenantId = TenantB, SettingKey = "smtp.host", SettingValue = "hijacked" };
        Assert.False(await repo.UpdateAsync(forged));

        _db.ChangeTracker.Clear();
        Assert.Equal("mail.b.test", (await _db.SystemSettings.SingleAsync(s => s.Id == _settingOfB.Id)).SettingValue);
    }

    [Fact]
    public async Task Settings_NewRowsAreStampedWithTheCallersTenantWhateverTheCallerSent()
    {
        var repo = new SystemSettingRepository(_db, Tenant(TenantA));

        var id = await repo.CreateAsync(new SystemSetting { TenantId = TenantB, SettingKey = "x", SettingValue = "1" });

        Assert.Equal(TenantA, (await _db.SystemSettings.SingleAsync(s => s.Id == id)).TenantId);
    }

    [Fact]
    public async Task NoTenantInTheRequest_FailsClosed()
    {
        var repo = new SystemSettingRepository(_db, Tenant(null));

        await Assert.ThrowsAsync<UnauthorizedException>(() => repo.GetAllAsync());
    }

    [Fact]
    public async Task AuditLogs_AreScoped_UnlessAllTenantsIsRequested()
    {
        var repo = new AuditLogRepository(_db, Tenant(TenantA));

        Assert.Single(await repo.GetAllAsync());
        Assert.Empty(await repo.GetByEntityAsync("Customer", "2"));
        Assert.Equal(1, (await repo.GetPagedAsync(new AuditLogPagedQuery())).TotalCount);
        Assert.Equal(3, (await repo.GetPagedAsync(new AuditLogPagedQuery(), allTenants: true)).TotalCount);
    }

    [Fact]
    public async Task AuditLogs_FilterAndDefaultToNewestFirst()
    {
        var repo = new AuditLogRepository(_db, Tenant(TenantB));

        var deleted = await repo.GetPagedAsync(new AuditLogPagedQuery { Action = "Deleted" });
        Assert.Equal("3", Assert.Single(deleted.Items).EntityId);
    }

    [Fact]
    public async Task SystemLogs_AreScoped()
    {
        var repo = new SystemLogRepository(_db, Tenant(TenantB));

        var page = await repo.GetPagedAsync(new SystemLogPagedQuery { LogLevel = "Error" });
        Assert.Equal("B failed", Assert.Single(page.Items).Message);
    }

    private static ITenantProvider Tenant(Guid? id) => new FixedTenant(id);

    private sealed class FixedTenant(Guid? id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
