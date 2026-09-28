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

    private IQueryable<SalesOrder> SalesOrdersQuery() =>
        ApplyTenant(Context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines));

    public async Task<SalesOrder?> GetByIdAsync(Guid id) =>
        await SalesOrdersQuery().FirstOrDefaultAsync(so => so.Id == id);

    public async Task<IEnumerable<SalesOrder>> GetAllAsync() =>
        await SalesOrdersQuery().ToListAsync();

    public async Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId) =>
        await SalesOrdersQuery().Where(so => so.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status) =>
        await SalesOrdersQuery().Where(so => so.Status == status).ToListAsync();

    public async Task<SalesOrder?> GetByNumberAsync(string salesOrderNumber) =>
        await SalesOrdersQuery().FirstOrDefaultAsync(so =>
            so.OrderNumber == salesOrderNumber || so.SalesOrderNumber == salesOrderNumber);

    public async Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        await SalesOrdersQuery().Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate).ToListAsync();

    public async Task<Guid> CreateAsync(SalesOrder salesOrder)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty)
            salesOrder.TenantId = TenantId.Value;

        Context.SalesOrders.Add(salesOrder);
        await Context.SaveChangesAsync();
        return salesOrder.Id;
    }

    public async Task<bool> UpdateAsync(SalesOrder salesOrder)
    {
        if (!MatchesTenant(salesOrder)) return false;
        Context.SalesOrders.Update(salesOrder);
        return await Context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var salesOrder = await GetByIdAsync(id);
        if (salesOrder == null) return false;
        Context.SalesOrders.Remove(salesOrder);
        return await Context.SaveChangesAsync() > 0;
    }
}
