using ERP.Application.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Admin;

/// <summary>Checkpoint 2 finding F1: user procedures didn't filter by the caller's tenant.</summary>
public class UserIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly User _userOfB;
    private readonly Role _roleOfA;
    private readonly Role _roleOfB;

    public UserIsolationTests()
    {
        using var db = Db();
        db.Users.Add(NewUser(TenantA, "a@x.test"));
        _userOfB = NewUser(TenantB, "b@x.test");
        db.Users.Add(_userOfB);
        _roleOfA = new Role { TenantId = TenantA, RoleName = "A users", IsActive = true };
        _roleOfB = new Role { TenantId = TenantB, RoleName = "B admins", IsActive = true };
        db.Roles.Add(_roleOfA);
        db.Roles.Add(_roleOfB);
        db.SaveChanges();
    }

    private ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(_dbName).Options);

    private static User NewUser(Guid tenant, string email) => new()
    {
        TenantId = tenant, Username = email, Email = email, PrimaryEmail = email, FirstName = "F", LastName = "L",
        FullName = "F L", PasswordHash = "h", PasswordSalt = "s",
    };

    private (UserService service, ERPDbContext db) ServiceFor(Guid tenant)
    {
        var db = Db();
        var repo = new UserRepository(db, new FixedTenant(tenant));
        return (new UserService(repo, new UserRoleRepository(db), db), db);
    }

    [Fact]
    public async Task ListsAndReadsOnlyTheCallersUsers()
    {
        var (service, _) = ServiceFor(TenantA);

        var all = (await service.GetAllAsync()).ToList();
        Assert.Equal("a@x.test", Assert.Single(all).Email);
        Assert.Null(await service.GetByIdAsync(_userOfB.Id));
    }

    [Fact]
    public async Task CannotDeleteOrResetAnotherTenantsUser()
    {
        var (service, _) = ServiceFor(TenantA);

        Assert.False(await service.DeleteAsync(_userOfB.Id));
        Assert.False(await service.ResetPasswordAsync(new ResetUserPasswordDto { UserId = _userOfB.Id, NewPassword = "Hijacked123!" }));

        await using var check = Db();
        var b = await check.Users.SingleAsync(u => u.Id == _userOfB.Id);
        Assert.Equal("h", b.PasswordHash);
    }

    [Fact]
    public async Task NewUsersLandInTheCallersTenant_WhateverTheRequestSays()
    {
        var (service, _) = ServiceFor(TenantA);

        var id = await service.CreateAsync(new CreateUserDto
        {
            TenantId = TenantB,
            FirstName = "N",
            LastName = "U",
            Email = "new@x.test",
            Password = "Passw0rd!long",
            RoleIds = [_roleOfA.Id],
        });

        await using var check = Db();
        Assert.Equal(TenantA, (await check.Users.SingleAsync(u => u.Id == id)).TenantId);
    }

    [Fact]
    public async Task TheSameEmailMayExistInAnotherOrganization()
    {
        var (service, _) = ServiceFor(TenantA);

        // b@x.test already exists in tenant B; tenant A may reuse that address.
        await service.CreateAsync(new CreateUserDto
        {
            FirstName = "N",
            LastName = "U",
            Email = "b@x.test",
            Password = "Passw0rd!long",
            RoleIds = [_roleOfA.Id],
        });
        // a@x.test already exists in tenant A → rejected.
        await Assert.ThrowsAnyAsync<Exception>(() => service.CreateAsync(new CreateUserDto
        {
            FirstName = "N",
            LastName = "U",
            Email = "a@x.test",
            Password = "Passw0rd!long",
            RoleIds = [_roleOfA.Id],
        }));
    }

    [Fact]
    public async Task CannotAssignAnotherTenantsRole()
    {
        var (service, _) = ServiceFor(TenantA);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateUserDto
            {
                FirstName = "N",
                LastName = "U",
                Email = "r@x.test",
                Password = "Passw0rd!long",
                RoleIds = [_roleOfB.Id],
            }));
    }

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
