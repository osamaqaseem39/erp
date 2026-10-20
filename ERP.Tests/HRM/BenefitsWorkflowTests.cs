using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class BenefitsWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static BenefitsService Svc(ERPDbContext db) => new(new BenefitRepository(db));

    private static async Task SeedEmployeeAsync(ERPDbContext db)
    {
        db.Employees.Add(new Employee
        {
            Id = EmployeeId,
            TenantId = Tenant,
            EmployeeCode = "E-BEN",
            FirstName = "Ben",
            LastName = "Efit",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Enroll_Unenroll_SoftDelete()
    {
        await using var db = Db();
        await SeedEmployeeAsync(db);
        var svc = Svc(db);

        var created = await svc.CreateAsync(new CreateBenefitDto
        {
            Name = "Health Plus",
            Type = "Health",
            Cost = 120m,
            Description = "Medical"
        }, Tenant, User);
        Assert.NotNull(created.Id);
        Assert.Equal("Health Plus", created.Name);

        var active = (await svc.GetActiveAsync(Tenant)).ToList();
        Assert.Single(active);

        await svc.EnrollEmployeeAsync(created.Id!.Value, new EnrollEmployeeDto
        {
            BenefitId = created.Id.Value,
            EmployeeId = EmployeeId,
            EnrollmentDate = DateTime.UtcNow
        }, Tenant, User);

        var forEmp = (await svc.GetByEmployeeAsync(EmployeeId, Tenant)).ToList();
        Assert.Single(forEmp);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.EnrollEmployeeAsync(created.Id.Value, new EnrollEmployeeDto
            {
                EmployeeId = EmployeeId,
                EnrollmentDate = DateTime.UtcNow
            }, Tenant, User));

        await svc.UnenrollEmployeeAsync(created.Id.Value, new UnenrollEmployeeDto
        {
            BenefitId = created.Id.Value,
            EmployeeId = EmployeeId,
            UnenrollmentDate = DateTime.UtcNow,
            Reason = "Left plan"
        }, Tenant, User);

        Assert.Empty(await svc.GetByEmployeeAsync(EmployeeId, Tenant));

        Assert.True(await svc.DeleteAsync(created.Id.Value, Tenant));
        Assert.Null(await svc.GetByIdAsync(created.Id.Value, Tenant));
    }
}
