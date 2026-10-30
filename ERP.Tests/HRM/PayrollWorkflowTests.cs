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
        new EmployeeRepository(db),
        db);

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

    [Fact]
    public async Task Salary_Structure_Drives_Lines_And_Payslips()
    {
        await using var db = Db();
        await SeedAsync(db);
        var setup = new PayrollSetupService(db);
        var svc = Svc(db);

        var basic = await setup.SaveComponentAsync(new PayrollComponentDto { Name = "Basic", Type = "Basic", CalculationType = "Fixed", Value = 0 }, Tenant, User);
        var hra = await setup.SaveComponentAsync(new PayrollComponentDto { Name = "House rent", Type = "Earning", CalculationType = "PercentOfBasic", Value = 40, IsTaxable = true }, Tenant, User);
        var eobi = await setup.SaveComponentAsync(new PayrollComponentDto { Name = "EOBI", Type = "Deduction", CalculationType = "Fixed", Value = 100 }, Tenant, User);
        var tax = await setup.SaveComponentAsync(new PayrollComponentDto { Name = "Income tax", Type = "Tax", CalculationType = "PercentOfGross", Value = 5 }, Tenant, User);
        foreach (var (c, v) in new[] { (basic.Id, (decimal?)6000m), (hra.Id, null), (eobi.Id, null), (tax.Id, null) })
            await setup.SaveStructureAsync(new EmployeeSalaryComponentDto { EmployeeId = EmpA, PayrollComponentId = c, Value = v }, Tenant, User);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            setup.SaveStructureAsync(new EmployeeSalaryComponentDto { EmployeeId = EmpA, PayrollComponentId = hra.Id }, Tenant, User));

        var id = await svc.CreateAsync(new PayrollDto
        {
            EmployeeId = EmpA,
            PayPeriodStart = new DateTime(2026, 4, 1),
            PayPeriodEnd = new DateTime(2026, 4, 30),
            PayDate = new DateTime(2026, 5, 1),
        }, Tenant, User);
        Assert.True(await svc.ProcessPayrollAsync(id, Tenant, User));

        // Basic 6000 + HRA 2400 = 8400 gross; EOBI 100 + tax 5% of 8400 = 420, net 7880
        var run = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal(8400m, run!.TotalGross);
        Assert.Equal(7880m, run.TotalNet);

        var slip = Assert.Single(await setup.GetPayslipsAsync(id, Tenant));
        Assert.Equal(new[] { "Basic", "House rent" }, slip.Earnings.Select(e => e.Name));
        Assert.Equal(new[] { 100m, 420m }, slip.DeductionItems.Select(d => d.Amount));
        Assert.Equal(7880m, slip.NetPay);
    }

    [Fact]
    public void Calculator_Prorates_Partial_Period()
    {
        var basic = new PayrollComponent { Name = "Basic", Type = "Basic", CalculationType = "Fixed" };
        var r = SalaryCalculator.Calculate([new SalaryCalculator.Item(basic, 3000m)], 0, 0.5m);
        Assert.Equal(1500m, r.Gross);
    }
}
