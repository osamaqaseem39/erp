using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class PerformanceWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Emp = Guid.NewGuid();
    private static readonly Guid Reviewer = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static PerformanceService Svc(ERPDbContext db) => new(new PerformanceReviewRepository(db));

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.Employees.AddRange(
            new Employee
            {
                Id = Emp,
                TenantId = Tenant,
                EmployeeCode = "E-P1",
                FirstName = "Pat",
                LastName = "Employee",
                Status = "Active",
                IsActive = true
            },
            new Employee
            {
                Id = Reviewer,
                TenantId = Tenant,
                EmployeeCode = "E-P2",
                FirstName = "Rev",
                LastName = "Viewer",
                Status = "Active",
                IsActive = true
            });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Submit_Approve_Complete_And_TenantIsolation()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        var id = await svc.CreateAsync(new CreatePerformanceReviewDto
        {
            EmployeeId = Emp,
            ReviewerId = Reviewer,
            ReviewPeriodStart = new DateTime(2026, 1, 1),
            ReviewPeriodEnd = new DateTime(2026, 3, 31),
            OverallRating = 4.2m,
            Strengths = "Delivery"
        }, Tenant, User);

        var draft = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(draft);
        Assert.Equal("Draft", draft!.Status);
        Assert.Null(await svc.GetByIdAsync(id, OtherTenant));

        Assert.True(await svc.UpdateAsync(new UpdatePerformanceReviewDto
        {
            Id = id,
            Comments = "Mid-year notes",
            OverallRating = 4.5m
        }, Tenant, User));

        Assert.True(await svc.SubmitAsync(id, Tenant, User));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateAsync(new UpdatePerformanceReviewDto { Id = id, Comments = "x" }, Tenant, User));

        Assert.True(await svc.ApproveAsync(id, Tenant, User));
        Assert.True(await svc.CompleteAsync(id, Tenant, User));

        var done = await svc.GetByIdAsync(id, Tenant);
        Assert.Equal("Completed", done!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Delete_Only_Draft()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        var id = await svc.CreateAsync(new CreatePerformanceReviewDto
        {
            EmployeeId = Emp,
            ReviewerId = Reviewer,
            ReviewPeriodStart = DateTime.UtcNow.Date.AddMonths(-3),
            ReviewPeriodEnd = DateTime.UtcNow.Date
        }, Tenant, User);

        Assert.True(await svc.DeleteAsync(id, Tenant));
        Assert.Null(await svc.GetByIdAsync(id, Tenant));
    }
}
