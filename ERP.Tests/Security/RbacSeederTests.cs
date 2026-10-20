using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Security;

public class RbacSeederTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();

    private readonly FakeCatalog _catalog = new(
        new PermissionDefinition("crm.customers", "view", "View customers"),
        new PermissionDefinition("crm.customers", "create", "Create customers"),
        new PermissionDefinition("finance.invoices", "view", "View invoices"),
        new PermissionDefinition("finance.invoices", "approve", "Invoices: approve"));

    private readonly ServiceProvider _services;

    public RbacSeederTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<IEffectivePermissions, EffectivePermissions>();
        services.AddSingleton<PermissionCacheInvalidator>();
        services.AddDbContext<ERPDbContext>((sp, o) => o.UseInMemoryDatabase(dbName).AddInterceptors(sp.GetRequiredService<PermissionCacheInvalidator>()));
        _services = services.BuildServiceProvider();
    }

    private ERPDbContext Db() => _services.CreateScope().ServiceProvider.GetRequiredService<ERPDbContext>();

    private RbacSeeder Seeder(ERPDbContext db) => new(db, _catalog, NullLogger<RbacSeeder>.Instance);

    private async Task<User> AddUser(ERPDbContext db, Guid tenant, string email)
    {
        var user = new User
        {
            TenantId = tenant, Username = email, Email = email, PrimaryEmail = email, FirstName = "F", LastName = "L",
            FullName = "F L", PasswordHash = "h", PasswordSalt = "s",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private async Task Grant(ERPDbContext db, User user, string roleName)
    {
        var role = await db.Roles.SingleAsync(r => r.TenantId == user.TenantId && r.RoleName == roleName);
        db.UserRoles.Add(new UserRole { TenantId = user.TenantId, UserId = user.Id, RoleId = role.Id });
        await db.SaveChangesAsync();
    }

    private async Task<IReadOnlySet<string>> PermissionsOf(User user) =>
        await _services.GetRequiredService<IEffectivePermissions>().GetAsync(user.TenantId, user.Id);

    [Fact]
    public async Task Creates_permissions_and_built_in_roles()
    {
        await using var db = Db();
        var result = await Seeder(db).SeedTenantAsync(Tenant);

        Assert.Equal(4, result.PermissionsAdded);
        // Administrator + Read-only + Sales/Finance/Accountant/HR/Operations/Warehouse/Production
        Assert.Equal(9, result.RolesCreated);
        var roles = await db.Roles.Where(r => r.TenantId == Tenant).ToListAsync();
        Assert.True(roles.Single(r => r.RoleName == RbacSeeder.Administrator).IsSystemRole);
        Assert.True(roles.Single(r => r.RoleName == RbacSeeder.ReadOnly).IsSystemRole);
        Assert.False(roles.Single(r => r.RoleName == "Sales").IsSystemRole);
        Assert.Contains(roles, r => r.RoleName == "Accountant");
        Assert.Contains(roles, r => r.RoleName == "Warehouse");
        Assert.Contains(roles, r => r.RoleName == "Production");
        Assert.Empty(await db.Permissions.Where(p => p.TenantId == OtherTenant).ToListAsync());
    }

    [Fact]
    public async Task Is_idempotent()
    {
        await using var db = Db();
        await Seeder(db).SeedTenantAsync(Tenant);
        var again = await Seeder(db).SeedTenantAsync(Tenant);

        Assert.Equal(new RbacSeedResult(0, 0, 0), again);
        Assert.Equal(4, await db.Permissions.CountAsync());
        Assert.Equal(9, await db.Roles.CountAsync());
    }

    [Fact]
    public async Task Read_only_views_administrator_does_everything_templates_cover_their_modules()
    {
        await using var db = Db();
        var viewer = await AddUser(db, Tenant, "viewer@x.test");
        var admin = await AddUser(db, Tenant, "admin@x.test");
        var sales = await AddUser(db, Tenant, "sales@x.test");
        await Seeder(db).SeedTenantAsync(Tenant);
        db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.UserId != viewer.Id));
        await db.SaveChangesAsync();
        await Grant(db, admin, RbacSeeder.Administrator);
        await Grant(db, sales, "Sales");

        Assert.Equal(["crm.customers.view", "finance.invoices.view"], (await PermissionsOf(viewer)).Order());
        Assert.Equal(4, (await PermissionsOf(admin)).Count);
        Assert.Equal(["crm.customers.create", "crm.customers.view"], (await PermissionsOf(sales)).Order());
    }

    [Fact]
    public async Task Users_without_a_role_get_read_only_once()
    {
        await using var db = Db();
        var roleless = await AddUser(db, Tenant, "nobody@x.test");
        var elsewhere = await AddUser(db, OtherTenant, "other@x.test");

        var result = await Seeder(db).SeedTenantAsync(Tenant);

        Assert.Equal(1, result.UsersGivenReadOnly);
        Assert.Contains("crm.customers.view", await PermissionsOf(roleless));
        Assert.Empty(await PermissionsOf(elsewhere));

        // Removing it later is respected: the next run doesn't hand it back.
        db.UserRoles.RemoveRange(db.UserRoles.Where(ur => ur.UserId == roleless.Id));
        await db.SaveChangesAsync();
        Assert.Equal(0, (await Seeder(db).SeedTenantAsync(Tenant)).UsersGivenReadOnly);
    }

    [Fact]
    public async Task New_endpoints_reach_system_roles_but_not_edited_templates()
    {
        await using var db = Db();
        await Seeder(db).SeedTenantAsync(Tenant);
        var sales = await db.Roles.SingleAsync(r => r.RoleName == "Sales");
        db.RolePermissions.RemoveRange(db.RolePermissions.Where(rp => rp.RoleId == sales.Id).Take(1));
        await db.SaveChangesAsync();

        _catalog.Add(new PermissionDefinition("crm.leads", "view", "View leads"));
        var result = await Seeder(db).SeedTenantAsync(Tenant);

        Assert.Equal(1, result.PermissionsAdded);
        var leadsView = await db.Permissions.SingleAsync(p => p.ModuleName == "crm.leads");
        var holders = await db.RolePermissions.Where(rp => rp.PermissionId == leadsView.Id).Select(rp => rp.Role.RoleName).ToListAsync();
        Assert.Equal([RbacSeeder.Administrator, RbacSeeder.ReadOnly], holders.Order());
    }

    [Fact]
    public async Task Removed_endpoints_stop_granting()
    {
        await using var db = Db();
        var admin = await AddUser(db, Tenant, "admin@x.test");
        await Seeder(db).SeedTenantAsync(Tenant);
        await Grant(db, admin, RbacSeeder.Administrator);
        Assert.Contains("finance.invoices.approve", await PermissionsOf(admin));

        _catalog.Remove("finance.invoices.approve");
        await Seeder(db).SeedTenantAsync(Tenant);

        Assert.DoesNotContain("finance.invoices.approve", await PermissionsOf(admin));
    }

    [Fact]
    public async Task Role_changes_take_effect_immediately()
    {
        await using var db = Db();
        var user = await AddUser(db, Tenant, "u@x.test");
        await Seeder(db).SeedTenantAsync(Tenant);
        Assert.DoesNotContain("crm.customers.create", await PermissionsOf(user)); // now cached

        await Grant(db, user, RbacSeeder.Administrator);

        Assert.Contains("crm.customers.create", await PermissionsOf(user));
    }

    private sealed class FakeCatalog : IPermissionCatalog
    {
        private readonly List<PermissionDefinition> _permissions;
        public FakeCatalog(params PermissionDefinition[] permissions) => _permissions = permissions.ToList();
        public IReadOnlyList<PermissionDefinition> Permissions => _permissions;
        public void Add(PermissionDefinition permission) => _permissions.Add(permission);
        public void Remove(string code) => _permissions.RemoveAll(p => p.Code == code);
    }
}
