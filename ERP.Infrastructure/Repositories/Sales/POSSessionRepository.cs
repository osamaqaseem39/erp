using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Sales;

public class POSSessionRepository : IPOSSessionRepository
{
    private readonly ERPDbContext _context;

    public POSSessionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<POSSession?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.POSSessions
            .FirstOrDefaultAsync(ps => ps.Id == id && ps.TenantId == tenantId);
    }

    public async Task<IEnumerable<POSSession>> GetAllAsync(Guid tenantId)
    {
        return await _context.POSSessions
            .Where(ps => ps.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<POSSession> AddAsync(POSSession entity)
    {
        _context.POSSessions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(POSSession entity)
    {
        _context.POSSessions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity != null)
        {
            _context.POSSessions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<POSSession>> GetByCashierAsync(Guid cashierId, Guid tenantId)
    {
        return await _context.POSSessions
            .Where(ps => ps.CashierId == cashierId && ps.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<POSSession>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.POSSessions
            .Where(ps => ps.Status == status && ps.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<bool> CloseSessionAsync(Guid sessionId, decimal closingBalance, string? notes, Guid tenantId, Guid userId)
    {
        var entity = await GetByIdAsync(sessionId, tenantId);
        if (entity == null) return false;

        entity.EndTime = DateTime.UtcNow;
        entity.ClosingBalance = closingBalance;
        entity.ExpectedBalance = entity.OpeningBalance + (await _context.POSTransactions
            .Where(pt => pt.POSSessionId == sessionId && pt.TenantId == tenantId)
            .SumAsync(pt => pt.TotalAmount));
        entity.Difference = closingBalance - entity.ExpectedBalance;
        entity.Status = "Closed";
        entity.Notes = notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
