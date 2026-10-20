using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class AttendanceWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AttendanceService Svc(ERPDbContext db) =>
        new(new TimeEntryRepositoryAdapter(new TimeEntryRepository(db)));

    private static async Task SeedEmployeeAsync(ERPDbContext db)
    {
        db.Employees.Add(new Employee
        {
            Id = EmployeeId,
            TenantId = Tenant,
            EmployeeCode = "E-ATT",
            FirstName = "Grace",
            LastName = "Hopper",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CheckIn_CheckOut_Summary_And_Adjust()
    {
        await using var db = Db();
        await SeedEmployeeAsync(db);
        var svc = Svc(db);

        var day = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
        var cin = await svc.CheckInAsync(new CheckInDto
        {
            EmployeeId = EmployeeId,
            CheckInTime = day.AddHours(9),
            Location = "HQ"
        }, Tenant, User);

        Assert.Equal("Active", cin.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckInAsync(new CheckInDto
            {
                EmployeeId = EmployeeId,
                CheckInTime = day.AddHours(10)
            }, Tenant, User));

        var cout = await svc.CheckOutAsync(new CheckOutDto
        {
            EmployeeId = EmployeeId,
            CheckOutTime = day.AddHours(17)
        }, Tenant, User);
        Assert.Equal("Completed", cout.Status);
        Assert.Equal(8m, cout.HoursWorked);

        var summary = await svc.GetAttendanceSummaryAsync(EmployeeId, Tenant, day, day);
        Assert.Equal(1, summary.DaysPresent);
        Assert.Equal(8m, summary.TotalHours);

        var adjusted = await svc.AdjustAttendanceAsync(new AttendanceAdjustmentDto
        {
            EmployeeId = EmployeeId,
            Date = day,
            CheckInTime = day.AddHours(8),
            CheckOutTime = day.AddHours(17),
            Reason = "Forgot early start"
        }, Tenant, User);
        Assert.Equal(9m, adjusted.HoursWorked);
    }

    [Fact]
    public async Task Empty_Tenant_Rejected_On_CheckIn()
    {
        await using var db = Db();
        await SeedEmployeeAsync(db);
        var svc = Svc(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.CheckInAsync(new CheckInDto
            {
                EmployeeId = EmployeeId,
                CheckInTime = DateTime.UtcNow
            }, Guid.Empty, User));
    }
}
