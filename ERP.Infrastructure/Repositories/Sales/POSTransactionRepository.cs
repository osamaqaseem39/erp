using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSTransactionRepository : IPOSTransactionRepository
{
    private readonly ERPDbContext _context;

    public POSTransactionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<POSTransaction?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.POSTransactions
            .FirstOrDefaultAsync(pt => pt.Id == id && pt.TenantId == tenantId);
    }

    public async Task<IEnumerable<POSTransaction>> GetAllAsync(Guid tenantId)
    {
        return await _context.POSTransactions
            .Where(pt => pt.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<POSTransaction> AddAsync(POSTransaction entity)
    {
        _context.POSTransactions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(POSTransaction entity)
    {
        _context.POSTransactions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity != null)
        {
            _context.POSTransactions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<POSTransaction>> GetBySessionAsync(Guid sessionId, Guid tenantId)
    {
        return await _context.POSTransactions
            .Where(pt => pt.POSSessionId == sessionId && pt.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<POSTransaction>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        return await _context.POSTransactions
            .Where(pt => pt.CustomerId == customerId && pt.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<POSTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        return await _context.POSTransactions
            .Where(pt => pt.TransactionDate >= startDate && pt.TransactionDate <= endDate && pt.TenantId == tenantId)
            .ToListAsync();
    }
}
