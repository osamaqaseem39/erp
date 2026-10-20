using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class SalesOrderLineRepository : CrmRepositoryBase, ISalesOrderLineRepository
{
    public SalesOrderLineRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    public async Task<IEnumerable<SalesOrderLine>> GetBySalesOrderIdAsync(Guid salesOrderId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await Context.SalesOrderLines
            .Where(l => l.SalesOrderId == salesOrderId && l.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalesOrderLine line)
    {
        if (line.TenantId == Guid.Empty) return Guid.Empty;
        if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
        if (line.CreatedAt == default) line.CreatedAt = DateTime.UtcNow;
        Context.SalesOrderLines.Add(line);
        await Context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteBySalesOrderIdAsync(Guid salesOrderId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var lines = await Context.SalesOrderLines
            .Where(l => l.SalesOrderId == salesOrderId && l.TenantId == tenantId)
            .ToListAsync();
        if (lines.Count == 0) return true;
        Context.SalesOrderLines.RemoveRange(lines);
        return await Context.SaveChangesAsync() > 0;
    }
}
