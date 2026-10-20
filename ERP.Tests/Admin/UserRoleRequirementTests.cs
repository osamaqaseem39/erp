using ERP.Application.Admin;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Admin;

public class UserRoleRequirementTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ERPDbContext Db() =>
        new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(UserService Svc, Guid RoleId)> ArrangeAsync(ERPDbContext db)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            RoleName = "Warehouse",
            Description = "Warehouse",
            IsActive = true,
            IsSystemRole = false,
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var users = new UserRepository(db, new FixedTenant(Tenant));
        var userRoles = new UserRoleRepository(db);
        var svc = new UserService(users, userRoles, db, new SecurityPolicyProvider(db));
        return (svc, role.Id);
    }

    [Fact]
    public async Task CreateRequiresAtLeastOneRole()
    {
        await using var db = Db();
        var (svc, _) = await ArrangeAsync(db);

        await Assert.ThrowsAsync<ValidationException>(() => svc.CreateAsync(new CreateUserDto
        {
            FirstName = "A",
            LastName = "B",
            Email = "a@test.local",
            Password = "Password1!",
            RoleIds = new(),
        }));
    }

    [Fact]
    public async Task CreateAssignsRolesAndReturnsThemOnGet()
    {
        await using var db = Db();
        var (svc, roleId) = await ArrangeAsync(db);

        var id = await svc.CreateAsync(new CreateUserDto
        {
            FirstName = "Aslam",
            LastName = "Khan",
            Email = "aslam@test.local",
            Password = "Password1!",
            RoleIds = new() { roleId },
        });

        var dto = await svc.GetByIdAsync(id);
        Assert.NotNull(dto);
        Assert.Equal(new[] { roleId }, dto!.RoleIds);
        Assert.Equal(new[] { "Warehouse" }, dto.RoleNames);
    }

    [Fact]
    public async Task CreatePersistsPhonesAddressAndPicture()
    {
        await using var db = Db();
        var (svc, roleId) = await ArrangeAsync(db);

        var id = await svc.CreateAsync(new CreateUserDto
        {
            FirstName = "Aslam",
            LastName = "Khan",
            Email = "aslam@test.local",
            Username = "aslam.k",
            Password = "Password1!",
            Phone = "111",
            SecondaryPhone = "222",
            MobilePhone = "333",
            AddressLine1 = "12 Market St",
            City = "Lahore",
            ProfilePicture = "https://cdn.example/avatar.png",
            RoleIds = new() { roleId },
        });

        var dto = await svc.GetByIdAsync(id);
        Assert.Equal("aslam.k", dto!.Username);
        Assert.Equal("111", dto.Phone);
        Assert.Equal("222", dto.SecondaryPhone);
        Assert.Equal("333", dto.MobilePhone);
        Assert.Equal("12 Market St", dto.AddressLine1);
        Assert.Equal("Lahore", dto.City);
        Assert.Equal("https://cdn.example/avatar.png", dto.ProfilePicture);
    }

    [Fact]
    public async Task DuplicateEmailInTenantIsRejected()
    {
        await using var db = Db();
        var (svc, roleId) = await ArrangeAsync(db);

        await svc.CreateAsync(new CreateUserDto
        {
            FirstName = "A",
            LastName = "One",
            Email = "dup@test.local",
            Password = "Password1!",
            RoleIds = new() { roleId },
        });

        await Assert.ThrowsAsync<ValidationException>(() => svc.CreateAsync(new CreateUserDto
        {
            FirstName = "B",
            LastName = "Two",
            Email = "dup@test.local",
            Password = "Password1!",
            RoleIds = new() { roleId },
        }));
    }

    private sealed class FixedTenant(Guid id) : ERP.Shared.Abstractions.ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
