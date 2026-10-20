using ERP.Api.GraphQL.Admin;
using ERP.Application.Admin;
using ERP.Application.Core;
using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Admin;

public class GraphQLAdminUserEmailPermissionsSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesActivateDeactivateAdminEmailAndRolePermissions()
    {
        var queries = typeof(AdminQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(AdminQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(AdminMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(AdminMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetAdminEmail", queries);
        Assert.Contains("GetAdminEmails", queries);
        Assert.Contains("GetRolePermissions", queries);

        Assert.Contains("ActivateUser", mutations);
        Assert.Contains("DeactivateUser", mutations);
        Assert.Contains("AddAdminEmail", mutations);
        Assert.Contains("UpdateAdminEmail", mutations);
        Assert.Contains("DeleteAdminEmail", mutations);
        Assert.Contains("AssignRolePermissions", mutations);
    }

    [Fact]
    public async Task GraphQL_Activate_Deactivate_User()
    {
        await using var db = Db();
        var (hash, salt) = PasswordHelper.HashPassword("Correct-horse-1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            Username = "gql@x.test",
            Email = "gql@x.test",
            PrimaryEmail = "gql@x.test",
            FirstName = "G",
            LastName = "Q",
            FullName = "G Q",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tenant = new FixedTenant();
        var svc = new UserService(
            new UserRepository(db, tenant),
            new UserRoleRepository(db),
            db,
            new SecurityPolicyProvider(db),
            new SessionStore(db, new SecurityPolicyProvider(db)));
        var gql = new AdminMutations();

        Assert.True(await gql.DeactivateUser(svc, user.Id));
        Assert.False((await db.Users.SingleAsync()).IsActive);
        Assert.True(await gql.ActivateUser(svc, user.Id));
        Assert.True((await db.Users.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task GraphQL_AdminEmail_Create_List_Update()
    {
        await using var db = Db();
        var emailSvc = new AdminEmailService(new AdminEmailRepository(db));
        var gql = new AdminMutations();
        var queries = new AdminQueries();
        var userId = Guid.NewGuid();

        var created = await gql.AddAdminEmail(emailSvc, new CreateAdminEmailDto
        {
            Email = "notify@org.test",
            Type = "Finance"
        }, Tenant, userId);
        Assert.Equal("notify@org.test", created.Email);
        Assert.Equal("Finance", created.Type);

        var listed = (await queries.GetAdminEmails(emailSvc, Tenant, isActive: true)).ToList();
        Assert.Contains(listed, e => e.Id == created.Id);

        var updated = await gql.UpdateAdminEmail(emailSvc, created.Id, new UpdateAdminEmailDto
        {
            Type = "Support"
        }, Tenant, userId);
        Assert.Equal("Support", updated.Type);

        Assert.True(await gql.DeleteAdminEmail(emailSvc, created.Id, Tenant, userId));
        var afterDelete = await queries.GetAdminEmail(emailSvc, created.Id, Tenant);
        Assert.NotNull(afterDelete);
        Assert.False(afterDelete!.IsActive);
    }
}
