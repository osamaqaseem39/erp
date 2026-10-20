using System.Text.Json;
using ERP.Domain.Admin;
using ERP.Domain.Core;
using ERP.Domain.CRM;
using ERP.Domain.Procurement;
using ERP.Domain.SCM;
using ERP.Infrastructure.Auditing;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class AuditInterceptorTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Actor = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();

    private ERPDbContext Db(Guid? userId = null) => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(_dbName)
        .AddInterceptors(new AuditSaveChangesInterceptor(new FixedAudit(userId, Tenant)))
        .Options);

    private ERPDbContext Signed() => Db(Actor);

    [Fact]
    public async Task Create_RecordsWhoAndTheNewValues()
    {
        await using (var db = Signed())
        {
            db.Customers.Add(new Customer { TenantId = Tenant, CustomerCode = "C-001", CustomerName = "Acme", CompanyName = "Acme" });
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        var log = Assert.Single(read.AuditLogs);
        Assert.Equal(("Created", "Customer", Actor, Tenant), (log.Action, log.EntityName, log.UserId, log.TenantId));
        var values = JsonDocument.Parse(log.NewValues).RootElement;
        Assert.Equal("C-001", values.GetProperty("CustomerCode").GetString());
        Assert.Equal(string.Empty, log.OldValues);
    }

    [Fact]
    public async Task Update_RecordsOnlyTheChangedFields_BeforeAndAfter()
    {
        var id = await SeedCustomerAsync();
        await using (var db = Signed())
        {
            var customer = await db.Customers.SingleAsync(c => c.Id == id);
            customer.Status = "Suspended";
            customer.UpdatedAt = DateTime.UtcNow; // bookkeeping, not reported
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Updated");
        Assert.Equal("{\"status\":\"Active\"}".ToLowerInvariant(), log.OldValues.ToLowerInvariant());
        Assert.Equal("{\"status\":\"Suspended\"}".ToLowerInvariant(), log.NewValues.ToLowerInvariant());
    }

    [Fact]
    public async Task BookkeepingOnlyUpdates_AreNotAudited()
    {
        var id = await SeedCustomerAsync();
        await using (var db = Signed())
        {
            (await db.Customers.SingleAsync(c => c.Id == id)).UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        Assert.DoesNotContain(read.AuditLogs, a => a.Action == "Updated");
    }

    [Fact]
    public async Task Delete_RecordsTheOldValues()
    {
        var id = await SeedCustomerAsync();
        await using (var db = Signed())
        {
            db.Customers.Remove(await db.Customers.SingleAsync(c => c.Id == id));
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Deleted");
        Assert.Contains("Acme", log.OldValues);
    }

    [Fact]
    public async Task SecretsAreNeverWrittenToTheAuditTrail()
    {
        await using (var db = Signed())
        {
            db.Users.Add(new User { TenantId = Tenant, Username = "a", Email = "a@x.test", PrimaryEmail = "a@x.test", FirstName = "A", LastName = "B", FullName = "A B", PasswordHash = "HASH-VALUE", PasswordSalt = "SALT-VALUE", RefreshToken = "TOKEN-VALUE" });
            db.SystemSettings.Add(new SystemSetting { TenantId = Tenant, SettingKey = "email.smtpPassword", SettingValue = "p@ss", IsEncrypted = true });
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        var everything = string.Join("\n", read.AuditLogs.Select(a => a.OldValues + a.NewValues));
        Assert.DoesNotContain("HASH-VALUE", everything);
        Assert.DoesNotContain("SALT-VALUE", everything);
        Assert.DoesNotContain("TOKEN-VALUE", everything);
        Assert.DoesNotContain("p@ss", everything);
        Assert.Contains(AuditSaveChangesInterceptor.Redacted, everything);
    }

    [Fact]
    public async Task ChangesWithoutASignedInUser_AreNotAttributedToAnyone()
    {
        await using (var db = Db(userId: null))
        {
            db.Customers.Add(new Customer { TenantId = Tenant, CustomerCode = "C-9", CustomerName = "Setup", CompanyName = "Setup" });
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        Assert.Empty(read.AuditLogs);
    }

    [Fact]
    public async Task LogTablesAreNotThemselvesAudited()
    {
        await using (var db = Signed())
        {
            db.SystemLogs.Add(new SystemLog { TenantId = Tenant, LogLevel = "Error", Message = "x" });
            await db.SaveChangesAsync();
        }

        await using var read = Db();
        Assert.Empty(read.AuditLogs);
    }

    [Fact]
    public async Task VendorRepository_Create_IsAudited()
    {
        await using (var db = Signed())
        {
            var repo = new VendorRepository(db);
            await repo.AddAsync(new Vendor
            {
                TenantId = Tenant,
                CompanyId = Company,
                VendorCode = "V-001",
                VendorName = "Steel Co"
            });
        }

        await using var read = Db();
        var log = Assert.Single(read.AuditLogs.Where(a => a.EntityName == "Vendor"));
        Assert.Equal(("Created", Actor, Tenant), (log.Action, log.UserId, log.TenantId));
        Assert.Contains("V-001", log.NewValues);
    }

    [Fact]
    public async Task VendorRepository_Update_IsAudited()
    {
        var id = await SeedVendorAsync();
        await using (var db = Signed())
        {
            var repo = new VendorRepository(db);
            var vendor = await repo.GetByIdAsync(id, Tenant);
            Assert.NotNull(vendor);
            vendor!.VendorName = "Steel Co Renamed";
            await repo.UpdateAsync(vendor);
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Updated" && a.EntityName == "Vendor");
        Assert.Contains("Steel Co Renamed", log.NewValues);
    }

    [Fact]
    public async Task VendorRepository_Delete_IsAudited()
    {
        var id = await SeedVendorAsync();
        await using (var db = Signed())
        {
            var repo = new VendorRepository(db);
            Assert.True(await repo.DeleteAsync(id, Tenant));
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Deleted" && a.EntityName == "Vendor");
        Assert.Contains("Steel Co", log.OldValues);
    }

    [Fact]
    public async Task WarehouseRepository_Create_IsAudited()
    {
        await using (var db = Signed())
        {
            var repo = new WarehouseRepository(db);
            await repo.AddAsync(new Warehouse
            {
                TenantId = Tenant,
                CompanyId = Company,
                WarehouseCode = "WH-001",
                WarehouseName = "Main Depot",
                Name = "Main Depot",
                City = "Karachi",
                State = "SD",
                Country = "PK",
                PostalCode = "74000",
                AddressLine1 = "1 Dock Rd"
            });
        }

        await using var read = Db();
        var log = Assert.Single(read.AuditLogs.Where(a => a.EntityName == "Warehouse"));
        Assert.Equal(("Created", Actor, Tenant), (log.Action, log.UserId, log.TenantId));
        Assert.Contains("WH-001", log.NewValues);
    }

    [Fact]
    public async Task WarehouseRepository_Update_IsAudited()
    {
        var id = await SeedWarehouseAsync();
        await using (var db = Signed())
        {
            var repo = new WarehouseRepository(db);
            var warehouse = await repo.GetByIdAsync(id, Tenant);
            Assert.NotNull(warehouse);
            warehouse!.WarehouseName = "Main Depot Renamed";
            warehouse.Name = "Main Depot Renamed";
            await repo.UpdateAsync(warehouse);
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Updated" && a.EntityName == "Warehouse");
        Assert.Contains("Main Depot Renamed", log.NewValues);
    }

    [Fact]
    public async Task WarehouseRepository_Delete_IsAudited()
    {
        var id = await SeedWarehouseAsync();
        await using (var db = Signed())
        {
            var repo = new WarehouseRepository(db);
            Assert.True(await repo.DeleteAsync(id, Tenant));
        }

        await using var read = Db();
        var log = await read.AuditLogs.SingleAsync(a => a.Action == "Deleted" && a.EntityName == "Warehouse");
        Assert.Contains("Main Depot", log.OldValues);
    }

    private async Task<Guid> SeedCustomerAsync()
    {
        await using var db = Db(userId: null);
        var customer = new Customer { TenantId = Tenant, CustomerCode = "C-001", CustomerName = "Acme", CompanyName = "Acme", Status = "Active" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer.Id;
    }

    private async Task<Guid> SeedVendorAsync()
    {
        await using var db = Db(userId: null);
        var vendor = new Vendor
        {
            TenantId = Tenant,
            CompanyId = Company,
            VendorCode = "V-001",
            VendorName = "Steel Co"
        };
        db.Vendors.Add(vendor);
        await db.SaveChangesAsync();
        return vendor.Id;
    }

    private async Task<Guid> SeedWarehouseAsync()
    {
        await using var db = Db(userId: null);
        var warehouse = new Warehouse
        {
            TenantId = Tenant,
            CompanyId = Company,
            WarehouseCode = "WH-001",
            WarehouseName = "Main Depot",
            Name = "Main Depot",
            City = "Karachi",
            State = "SD",
            Country = "PK",
            PostalCode = "74000",
            AddressLine1 = "1 Dock Rd"
        };
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync();
        return warehouse.Id;
    }

    private sealed class FixedAudit(Guid? userId, Guid tenantId) : IAuditContext
    {
        public Guid? UserId => userId;
        public Guid? TenantId => tenantId;
        public string? IpAddress => "127.0.0.1";
        public string? UserAgent => "tests";
        public bool IsSuperAdmin => false;
    }
}
