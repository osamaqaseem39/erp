using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly ERPDbContext _context;

    public SalesOrderRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrder?> GetByIdAsync(Guid id)
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<IEnumerable<SalesOrder>> GetAllAsync()
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .Where(so => so.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status)
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .Where(so => so.Status == status)
            .ToListAsync();
    }

    public async Task<SalesOrder?> GetByNumberAsync(string salesOrderNumber)
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.SalesOrderLines)
            .FirstOrDefaultAsync(so => so.SalesOrderNumber == salesOrderNumber);
    }

    public async Task<Guid> CreateAsync(SalesOrder salesOrder)
    {
        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync();
        return salesOrder.Id;
    }

    public async Task<bool> UpdateAsync(SalesOrder salesOrder)
    {
        _context.SalesOrders.Update(salesOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var salesOrder = await _context.SalesOrders.FindAsync(id);
        if (salesOrder == null) return false;

        _context.SalesOrders.Remove(salesOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}