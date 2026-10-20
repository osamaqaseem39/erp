using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class EmployeeTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static EmployeeService Svc(ERPDbContext db) =>
        new(new EmployeeRepositoryAdapter(db), db);

    [Fact]
    public async Task Create_SetsTenant_And_CrossTenantGetFails()
    {
        await using var db = Db();
        var svc = Svc(db);

        var created = await svc.AddEmployeeAsync(new CreateEmployeeDto
        {
            EmployeeCode = "E-A1",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = "Active"
        }, TenantA, User);

        Assert.Equal(TenantA, created.TenantId);
        Assert.NotNull(created.Id);
        var id = created.Id.Value;

        var ok = await svc.GetEmployeeByIdAsync(id, TenantA);
        Assert.Equal("E-A1", ok.EmployeeCode);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.GetEmployeeByIdAsync(id, TenantB));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.GetEmployeeByIdAsync(id, Guid.Empty));
    }

    [Fact]
    public async Task List_FailClosed_EmptyTenant_And_ScopedByTenant()
    {
        await using var db = Db();
        db.Employees.AddRange(
            new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                EmployeeCode = "A-1",
                FirstName = "A",
                LastName = "One",
                Status = "Active"
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = TenantB,
                EmployeeCode = "B-1",
                FirstName = "B",
                LastName = "Two",
                Status = "Active"
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var a = await svc.GetEmployeesAsync(new EmployeeQuery(), TenantA);
        Assert.Single(a.Items);
        Assert.Equal("A-1", a.Items[0].EmployeeCode);

        var empty = await svc.GetEmployeesAsync(new EmployeeQuery(), Guid.Empty);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalCount);
    }

    [Fact]
    public async Task CodeUniqueness_IsPerTenant()
    {
        await using var db = Db();
        var svc = Svc(db);

        await svc.AddEmployeeAsync(new CreateEmployeeDto
        {
            EmployeeCode = "SAME",
            FirstName = "A",
            LastName = "A",
            Status = "Active"
        }, TenantA, User);

        Assert.False(await svc.IsEmployeeCodeUniqueAsync("SAME", TenantA));
        Assert.True(await svc.IsEmployeeCodeUniqueAsync("SAME", TenantB));

        var other = await svc.AddEmployeeAsync(new CreateEmployeeDto
        {
            EmployeeCode = "SAME",
            FirstName = "B",
            LastName = "B",
            Status = "Active"
        }, TenantB, User);
        Assert.Equal(TenantB, other.TenantId);
    }
}
