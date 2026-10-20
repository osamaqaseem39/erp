using ERP.Application.Admin;
using ERP.Application.Core.Settings;
using ERP.Application.HRM;
using ERP.Domain.Core;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Admin;

public class UserEmployeeLinkTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ERPDbContext Db() =>
        new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(UserService Users, EmployeeService Employees, Guid RoleId, Guid EmployeeId)> ArrangeAsync(ERPDbContext db)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            RoleName = "Warehouse",
            IsActive = true,
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            EmployeeCode = "E-100",
            FirstName = "Aslam",
            LastName = "Khan",
            Status = "Active",
            IsDeleted = false,
        };
        db.Roles.Add(role);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var userRepo = new UserRepository(db, new FixedTenant(Tenant));
        var users = new UserService(userRepo, new UserRoleRepository(db), db, new SecurityPolicyProvider(db));
        var employees = new EmployeeService(new EmployeeRepositoryAdapter(db), db);
        return (users, employees, role.Id, employee.Id);
    }

    [Fact]
    public async Task CreateLinksEmployeeAndReturnsSummary()
    {
        await using var db = Db();
        var (users, _, roleId, employeeId) = await ArrangeAsync(db);

        var id = await users.CreateAsync(new CreateUserDto
        {
            FirstName = "Aslam",
            LastName = "Khan",
            Email = "aslam@test.local",
            Password = "Password1!",
            EmployeeId = employeeId,
            RoleIds = new() { roleId },
        });

        var dto = await users.GetByIdAsync(id);
        Assert.Equal(employeeId, dto!.EmployeeId);
        Assert.Equal("E-100", dto.EmployeeCode);
        Assert.Equal("Aslam Khan", dto.EmployeeName);
    }

    [Fact]
    public async Task CannotLinkSameEmployeeToTwoUsers()
    {
        await using var db = Db();
        var (users, _, roleId, employeeId) = await ArrangeAsync(db);

        await users.CreateAsync(new CreateUserDto
        {
            FirstName = "A",
            LastName = "One",
            Email = "a@test.local",
            Password = "Password1!",
            EmployeeId = employeeId,
            RoleIds = new() { roleId },
        });

        await Assert.ThrowsAsync<ValidationException>(() => users.CreateAsync(new CreateUserDto
        {
            FirstName = "B",
            LastName = "Two",
            Email = "b@test.local",
            Password = "Password1!",
            EmployeeId = employeeId,
            RoleIds = new() { roleId },
        }));
    }

    [Fact]
    public async Task DeleteEmployeeBlockedWhenLinked()
    {
        await using var db = Db();
        var (users, employees, roleId, employeeId) = await ArrangeAsync(db);

        await users.CreateAsync(new CreateUserDto
        {
            FirstName = "Aslam",
            LastName = "Khan",
            Email = "aslam@test.local",
            Password = "Password1!",
            EmployeeId = employeeId,
            RoleIds = new() { roleId },
        });

        await Assert.ThrowsAsync<ConflictException>(() => employees.DeleteEmployeeAsync(employeeId, Tenant));
    }

    private sealed class FixedTenant(Guid id) : ERP.Shared.Abstractions.ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
