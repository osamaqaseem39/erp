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

    public async Task<IEnumerable<SalesOrderLine>> GetBySalesOrderIdAsync(Guid salesOrderId) =>
        await ApplyTenant(Context.SalesOrderLines.Where(l => l.SalesOrderId == salesOrderId)).ToListAsync();

    public async Task<Guid> CreateAsync(SalesOrderLine line)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            line.TenantId = TenantId.Value;

        Context.SalesOrderLines.Add(line);
        await Context.SaveChangesAsync();
        return line.Id;
    }

    public async Task<bool> DeleteBySalesOrderIdAsync(Guid salesOrderId)
    {
        var lines = await ApplyTenant(Context.SalesOrderLines.Where(l => l.SalesOrderId == salesOrderId)).ToListAsync();
        if (lines.Count == 0) return true;

        Context.SalesOrderLines.RemoveRange(lines);
        return await Context.SaveChangesAsync() > 0;
    }
}
