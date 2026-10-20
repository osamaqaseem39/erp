using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class PayrollWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid EmpA = Guid.NewGuid();
    private static readonly Guid EmpB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static PayrollCrudService Svc(ERPDbContext db) => new(
        new PayrollRepository(db),
        new PayrollLineRepository(db),
        new EmployeeRepository(db));

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.Employees.AddRange(
            new Employee
            {
                Id = EmpA,
                TenantId = Tenant,
                EmployeeCode = "E-A",
                FirstName = "Ada",
                LastName = "Lovelace",
                Status = "Active",
                IsActive = true,
                EmploymentContract = new EmploymentContract
                {
                    Id = Guid.NewGuid(),
                    TenantId = Tenant,
                    EmployeeId = EmpA,
                    ContractType = "Permanent",
                    StartDate = new DateTime(2025, 1, 1),
                    BaseSalary = 5000m,
                    Currency = "USD",
                    PayFrequency = "Monthly",
                    Status = "Active",
                    IsActive = true
                }
            },
            new Employee
            {
                Id = EmpB,
                TenantId = Tenant,
                EmployeeCode = "E-B",
                FirstName = "Alan",
                LastName = "Turing",
                Status = "Active",
                IsActive = true,
                EmploymentContract = new EmploymentContract
                {
                    Id = Guid.NewGuid(),
                    TenantId = Tenant,
                    EmployeeId = EmpB,
                    ContractType = "Permanent",
                    StartDate = new DateTime(2025, 1, 1),
                    BaseSalary = 4000m,
                    Currency = "USD",
                    PayFrequency = "Monthly",
                    Status = "Active",
                    IsActive = true
                }
            });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Process_Post_Totals_And_Immutability()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        var start = new DateTime(2026, 4, 1);
        var end = new DateTime(2026, 4, 30);
        var id = await svc.CreateAsync(new PayrollDto
        {
            PayPeriodStart = start,
            PayPeriodEnd = end,
            PayDate = new DateTime(2026, 5, 5),
            Currency = "USD",
            Allowances = 200m,
            Deductions = 50m
        }, Tenant, User);

        var draft = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(draft);
        Assert.Equal("Draft", draft!.Status);

        Assert.True(await svc.ProcessPayrollAsync(id, Tenant, User));
        var processed = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal("Processed", processed!.Status);
        Assert.Equal(2, processed.EmployeeCount);

        // A: 5000+200=5200, tax 520, ded 50+520=570, net 4630
        // B: 4000+200=4200, tax 420, ded 50+420=470, net 3730
        Assert.Equal(9400m, processed.TotalGross);
        Assert.Equal(1040m, processed.TotalDeductions);
        Assert.Equal(8360m, processed.TotalNet);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateAsync(new PayrollDto
            {
                Id = id,
                PayPeriodStart = start,
                PayPeriodEnd = end,
                PayDate = end,
                Currency = "USD"
            }, Tenant, User));

        Assert.True(await svc.ProcessPayrollAsync(id, Tenant, User));
        var posted = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal("Posted", posted!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.ProcessPayrollAsync(id, Tenant, User));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task SingleEmployee_Uses_Default_When_No_Contract_Salary()
    {
        await using var db = Db();
        var emp = Guid.NewGuid();
        db.Employees.Add(new Employee
        {
            Id = emp,
            TenantId = Tenant,
            EmployeeCode = "E-C",
            FirstName = "No",
            LastName = "Contract",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();
        var svc = Svc(db);

        var id = await svc.CreateAsync(new PayrollDto
        {
            EmployeeId = emp,
            PayPeriodStart = new DateTime(2026, 5, 1),
            PayPeriodEnd = new DateTime(2026, 5, 31),
            PayDate = new DateTime(2026, 6, 5),
            BasicSalary = 3000m,
            Currency = "USD"
        }, Tenant, User);

        Assert.True(await svc.ProcessPayrollAsync(id, Tenant, User));
        var run = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal(1, run!.EmployeeCount);
        Assert.Equal(3000m, run.TotalGross);
        Assert.Equal(300m, run.TotalDeductions); // 10% tax
        Assert.Equal(2700m, run.TotalNet);
    }
}
