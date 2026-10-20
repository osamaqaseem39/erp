using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class SalesOrderRepository : CrmRepositoryBase, ISalesOrderRepository
{
    public SalesOrderRepository(ERPDbContext context, ITenantProvider tenantProvider)
        : base(context, tenantProvider)
    {
    }

    private IQueryable<SalesOrder> ForTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Context.SalesOrders.Where(_ => false);
        return Context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .Where(so => so.TenantId == tenantId);
    }

    public async Task<SalesOrder?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<IEnumerable<SalesOrder>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(so => so.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId).Where(so => so.Status == status).ToListAsync();
    }

    public async Task<SalesOrder?> GetByNumberAsync(string salesOrderNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await ForTenant(tenantId).FirstOrDefaultAsync(so =>
            so.OrderNumber == salesOrderNumber || so.SalesOrderNumber == salesOrderNumber);
    }

    public async Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return await ForTenant(tenantId)
            .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalesOrder salesOrder)
    {
        if (salesOrder.TenantId == Guid.Empty) return Guid.Empty;
        if (salesOrder.Id == Guid.Empty) salesOrder.Id = Guid.NewGuid();
        if (salesOrder.CreatedAt == default) salesOrder.CreatedAt = DateTime.UtcNow;
        Context.SalesOrders.Add(salesOrder);
        await Context.SaveChangesAsync();
        return salesOrder.Id;
    }

    public async Task<bool> UpdateAsync(SalesOrder salesOrder)
    {
        if (salesOrder.TenantId == Guid.Empty) return false;
        salesOrder.UpdatedAt = DateTime.UtcNow;
        Context.SalesOrders.Update(salesOrder);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var salesOrder = await ForTenant(tenantId).FirstOrDefaultAsync(so => so.Id == id);
        if (salesOrder == null) return false;
        Context.SalesOrders.Remove(salesOrder);
        return await Context.SaveChangesAsync() > 0;
    }
}
