using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class TimeEntryTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid EmployeeA = Guid.NewGuid();
    private static readonly Guid EmployeeB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static TimeTrackingService Tracking(ERPDbContext db) =>
        new(new TimeEntryRepositoryAdapter(new TimeEntryRepository(db)));

    private static AttendanceService Attendance(ERPDbContext db) =>
        new(new TimeEntryRepositoryAdapter(new TimeEntryRepository(db)));

    [Fact]
    public async Task TimeTracking_FailsClosed_And_Isolates()
    {
        await using var db = Db();
        var svc = Tracking(db);

        var idA = await svc.CreateAsync(new CreateTimeEntryDto
        {
            EmployeeId = EmployeeA,
            WorkDate = DateTime.UtcNow.Date,
            StartTime = DateTime.UtcNow,
            HoursWorked = 4,
            Status = "Completed"
        }, TenantA);
        Assert.NotEqual(Guid.Empty, idA);

        Assert.Null(await svc.GetByIdAsync(idA, TenantB));
        Assert.Null(await svc.GetByIdAsync(idA, Guid.Empty));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.Empty(await svc.GetByEmployeeAsync(EmployeeA, TenantB));
        Assert.Equal(0m, await svc.GetTotalHoursAsync(EmployeeA, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1), Guid.Empty));

        Assert.NotNull(await svc.GetByIdAsync(idA, TenantA));
        Assert.False(await svc.DeleteAsync(idA, TenantB));
        Assert.True(await svc.DeleteAsync(idA, TenantA));
    }

    [Fact]
    public async Task Attendance_EmptyTenant_Fails_And_CrossTenant_Hidden()
    {
        await using var db = Db();
        var svc = Attendance(db);
        var day = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.CheckInAsync(new CheckInDto
            {
                EmployeeId = EmployeeA,
                CheckInTime = day.AddHours(9)
            }, Guid.Empty, Guid.NewGuid()));

        var cin = await svc.CheckInAsync(new CheckInDto
        {
            EmployeeId = EmployeeA,
            CheckInTime = day.AddHours(9)
        }, TenantA, Guid.NewGuid());
        Assert.Equal(TenantA, cin.TenantId);

        Assert.Empty(await svc.GetEmployeeAttendanceAsync(EmployeeA, TenantB, day, day));
        Assert.Empty(await svc.GetEmployeeAttendanceAsync(EmployeeA, Guid.Empty, day, day));
        Assert.Equal(0, (await svc.GetAttendanceSummaryAsync(EmployeeA, Guid.Empty, day, day)).TotalEntries);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckOutAsync(new CheckOutDto
            {
                EmployeeId = EmployeeA,
                CheckOutTime = day.AddHours(17)
            }, TenantB, Guid.NewGuid()));
    }
}
