using ERP.Application.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class POSSessionWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Cashier = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_Forces_Open_Update_Ignores_Status_Close_Workflow()
    {
        await using var db = Db();
        var svc = new POSSessionService(new POSSessionRepository(db));

        var id = await svc.CreateAsync(new CreatePOSSessionDto
        {
            SessionNumber = "S-1",
            CashierId = Cashier,
            OpeningBalance = 100m,
            Status = "Closed"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.UpdateAsync(new UpdatePOSSessionDto
        {
            Id = id,
            SessionNumber = "S-1",
            CashierId = Cashier,
            Status = "Closed",
            OpeningBalance = 100m
        }, Tenant, User));
        Assert.Equal("Open", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await svc.CloseSessionAsync(new ClosePOSSessionDto
        {
            SessionId = id,
            ClosingBalance = 150m
        }, Tenant, User));
        Assert.Equal("Closed", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.False(await svc.CloseSessionAsync(new ClosePOSSessionDto
        {
            SessionId = id,
            ClosingBalance = 160m
        }, Tenant, User));
        Assert.False(await svc.UpdateAsync(new UpdatePOSSessionDto
        {
            Id = id,
            SessionNumber = "S-1",
            CashierId = Cashier,
            OpeningBalance = 100m
        }, Tenant, User));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
    }

    [Fact]
    public async Task Cross_Tenant_Denied()
    {
        await using var db = Db();
        var other = Guid.NewGuid();
        var id = Guid.NewGuid();
        db.POSSessions.Add(new POSSession
        {
            Id = id,
            TenantId = other,
            SessionNumber = "X",
            CashierId = Cashier,
            Status = "Open",
            StartTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = new POSSessionService(new POSSessionRepository(db));
        Assert.Null(await svc.GetByIdAsync(id, Tenant));
        Assert.False(await svc.CloseSessionAsync(new ClosePOSSessionDto { SessionId = id, ClosingBalance = 0 }, Tenant, User));
    }
}
