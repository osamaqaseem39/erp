using ERP.Api.GraphQL.HRM;
using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class GraphQLAttendanceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AttendanceService Svc(ERPDbContext db) =>
        new(new TimeEntryRepositoryAdapter(new TimeEntryRepository(db)));

    [Fact]
    public void Surface_ExposesAttendanceQueriesAndMutations()
    {
        var queries = typeof(HRMQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(HRMQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(HRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(HRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetEmployeeAttendance", queries);
        Assert.Contains("GetAttendanceSummary", queries);
        Assert.Contains("CheckIn", mutations);
        Assert.Contains("CheckOut", mutations);
        Assert.Contains("AdjustAttendance", mutations);
    }

    [Fact]
    public async Task GraphQL_CheckIn_CheckOut_Summary()
    {
        await using var db = Db();
        db.Employees.Add(new Employee
        {
            Id = EmployeeId,
            TenantId = Tenant,
            EmployeeCode = "E-GQL-ATT",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var mutations = new HRMMutations();
        var queries = new HRMQueries();
        var day = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var cin = await mutations.CheckIn(svc, new CheckInDto
        {
            EmployeeId = EmployeeId,
            CheckInTime = day.AddHours(9),
            Location = "HQ"
        }, Tenant, User);
        Assert.Equal("Active", cin.Status);

        var cout = await mutations.CheckOut(svc, new CheckOutDto
        {
            EmployeeId = EmployeeId,
            CheckOutTime = day.AddHours(17)
        }, Tenant, User);
        Assert.Equal("Completed", cout.Status);
        Assert.Equal(8m, cout.HoursWorked);

        var rows = (await queries.GetEmployeeAttendance(svc, EmployeeId, Tenant, day, day)).ToList();
        Assert.Single(rows);

        var summary = await queries.GetAttendanceSummary(svc, EmployeeId, Tenant, day, day);
        Assert.Equal(1, summary.DaysPresent);
        Assert.Equal(8m, summary.TotalHours);
    }
}
