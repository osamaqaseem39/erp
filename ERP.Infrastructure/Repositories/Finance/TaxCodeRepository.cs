using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class TaxCodeRepository : ITaxCodeRepository
{
    private readonly ERPDbContext _context;

    public TaxCodeRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<TaxCode> Query(Guid tenantId) =>
        tenantId == Guid.Empty
            ? _context.TaxCodes.Where(_ => false)
            : _context.TaxCodes
                .Include(tc => tc.Account)
                .Where(tc => tc.TenantId == tenantId);

    public async Task<TaxCode?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId)
            .FirstOrDefaultAsync(tc => tc.Id == id || tc.TaxCodeId == id);
    }

    public async Task<IEnumerable<TaxCode>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCode>();
        return await Query(tenantId).OrderBy(tc => tc.Code).ToListAsync();
    }

    public async Task<IEnumerable<TaxCode>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCode>();
        return await Query(tenantId).Where(tc => tc.IsActive).OrderBy(tc => tc.Code).ToListAsync();
    }

    public async Task<IEnumerable<TaxCode>> GetByTypeAsync(string taxType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCode>();
        return await Query(tenantId)
            .Where(tc => tc.TaxType == taxType)
            .OrderBy(tc => tc.Code)
            .ToListAsync();
    }

    public async Task<TaxCode?> GetByCodeAsync(string taxCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId)
            .FirstOrDefaultAsync(tc => tc.Code == taxCode);
    }

    public async Task<Guid> CreateAsync(TaxCode taxCode)
    {
        if (taxCode.TenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId is required.");
        _context.TaxCodes.Add(taxCode);
        await _context.SaveChangesAsync();
        return taxCode.Id;
    }

    public async Task<bool> UpdateAsync(TaxCode taxCode)
    {
        _context.TaxCodes.Update(taxCode);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var taxCode = await GetByIdAsync(id, tenantId);
        if (taxCode == null) return false;

        _context.TaxCodes.Remove(taxCode);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
