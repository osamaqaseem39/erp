using System.Security.Claims;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class AuditServiceEfTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AuditService Svc(ERPDbContext db, Guid? tenantId, Guid? userId)
    {
        var http = new DefaultHttpContext();
        if (tenantId.HasValue && userId.HasValue)
        {
            http.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("TenantId", tenantId.Value.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Email, "a@test.local")
            ], "test"));
        }
        return new AuditService(db, new HttpContextAccessor { HttpContext = http });
    }

    [Fact]
    public async Task LogAsync_Persists_Via_Ef()
    {
        await using var db = Db();
        var svc = Svc(db, TenantA, User);

        await svc.LogAsync("Custom", "Setting", "k1", oldValues: null, newValues: "{\"v\":1}");

        var row = Assert.Single(db.AuditLogs);
        Assert.Equal(TenantA, row.TenantId);
        Assert.Equal(User, row.UserId);
        Assert.Equal("Custom", row.Action);
        Assert.Equal("Setting", row.EntityName);
        Assert.Contains("\"v\":1", row.NewValues);
    }

    [Fact]
    public async Task LogAsync_Noop_Without_Auth_Context()
    {
        await using var db = Db();
        var svc = Svc(db, tenantId: null, userId: null);
        await svc.LogAsync("X", "Y", "1");
        Assert.Empty(db.AuditLogs);
    }

    [Fact]
    public async Task GetAuditLogs_Tenant_Isolated_And_Paged()
    {
        await using var db = Db();
        for (var i = 0; i < 3; i++)
        {
            db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                UserId = User,
                Action = "A",
                EntityName = "E",
                EntityId = i.ToString(),
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                CreatedAt = DateTime.UtcNow
            });
        }
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = TenantB,
            UserId = User,
            Action = "B",
            EntityName = "E",
            EntityId = "x",
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db, TenantA, User);
        var page1 = (await svc.GetAuditLogsAsync(TenantA, pageNumber: 1, pageSize: 2)).ToList();
        Assert.Equal(2, page1.Count);
        Assert.All(page1, x => Assert.Equal(TenantA, x.TenantId));

        Assert.Empty(await svc.GetAuditLogsAsync(Guid.Empty));
        Assert.Single(await svc.GetAuditLogsAsync(TenantB));
    }

    [Fact]
    public async Task GetById_Returns_Row()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.AuditLogs.Add(new AuditLog
        {
            Id = id,
            TenantId = TenantA,
            UserId = User,
            Action = "Login",
            EntityName = "User",
            EntityId = User.ToString(),
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db, TenantA, User);
        var dto = await svc.GetAuditLogByIdAsync(id);
        Assert.NotNull(dto);
        Assert.Equal("Login", dto!.Action);
        Assert.Null(await svc.GetAuditLogByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetById_FailsClosed_For_Other_Tenant()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.AuditLogs.Add(new AuditLog
        {
            Id = id,
            TenantId = TenantB,
            UserId = User,
            Action = "Login",
            EntityName = "User",
            EntityId = User.ToString(),
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = Svc(db, TenantA, User);
        Assert.Null(await svc.GetAuditLogByIdAsync(id));
    }
}
