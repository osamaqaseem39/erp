using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public interface IPouchTypeRepository
{
    Task<PouchType?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PouchType>> GetAllAsync(Guid tenantId, bool activeOnly = true);
    Task<Guid> CreateAsync(PouchType entity);
    Task<bool> UpdateAsync(PouchType entity);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
}

public interface IProductionProcessRepository
{
    Task<ProductionProcess?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionProcess>> GetAllAsync(Guid tenantId, bool activeOnly = true);
    Task<Guid> CreateAsync(ProductionProcess entity);
    Task<bool> UpdateAsync(ProductionProcess entity);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
}

public interface IFOHTypeRepository
{
    Task<FOHType?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<FOHType>> GetAllAsync(Guid tenantId, bool activeOnly = true);
    Task<Guid> CreateAsync(FOHType entity);
    Task<bool> UpdateAsync(FOHType entity);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
}

public interface ISaleEnquiryRepository
{
    Task<SaleEnquiry?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false);
    Task<IEnumerable<SaleEnquiry>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(SaleEnquiry entity);
    Task<bool> UpdateAsync(SaleEnquiry entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task ReplaceLinesAsync(Guid enquiryId, Guid tenantId, IEnumerable<SaleEnquiryLine> lines);
}

public interface ISalePreCostRepository
{
    Task<SalePreCost?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false);
    Task<IEnumerable<SalePreCost>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(SalePreCost entity);
    Task<bool> UpdateAsync(SalePreCost entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task ReplaceLinesAsync(Guid preCostId, Guid tenantId, IEnumerable<SalePreCostLine> lines);
}

public class PouchTypeRepository : IPouchTypeRepository
{
    private readonly ERPDbContext _db;
    public PouchTypeRepository(ERPDbContext db) => _db = db;

    public async Task<PouchType?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.PouchTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<PouchType>> GetAllAsync(Guid tenantId, bool activeOnly = true)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PouchType>();
        var q = _db.PouchTypes.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Guid> CreateAsync(PouchType entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;
        _db.PouchTypes.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PouchType entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.PouchTypes.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.Code = entity.Code;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.PouchTypes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        e.IsActive = false;
        e.UpdatedAt = DateTime.UtcNow;
        return await _db.SaveChangesAsync() > 0;
    }
}

public class ProductionProcessRepository : IProductionProcessRepository
{
    private readonly ERPDbContext _db;
    public ProductionProcessRepository(ERPDbContext db) => _db = db;

    public async Task<ProductionProcess?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.ProductionProcesses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<ProductionProcess>> GetAllAsync(Guid tenantId, bool activeOnly = true)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionProcess>();
        var q = _db.ProductionProcesses.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProductionProcess entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;
        _db.ProductionProcesses.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ProductionProcess entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.ProductionProcesses.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.Code = entity.Code;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.ProductionProcesses.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        e.IsActive = false;
        e.UpdatedAt = DateTime.UtcNow;
        return await _db.SaveChangesAsync() > 0;
    }
}

public class FOHTypeRepository : IFOHTypeRepository
{
    private readonly ERPDbContext _db;
    public FOHTypeRepository(ERPDbContext db) => _db = db;

    public async Task<FOHType?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.FOHTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<FOHType>> GetAllAsync(Guid tenantId, bool activeOnly = true)
    {
        if (tenantId == Guid.Empty) return Array.Empty<FOHType>();
        var q = _db.FOHTypes.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<Guid> CreateAsync(FOHType entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;
        _db.FOHTypes.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(FOHType entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.FOHTypes.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.Code = entity.Code;
        existing.Name = entity.Name;
        existing.Basis = entity.Basis;
        existing.Rate = entity.Rate;
        existing.Description = entity.Description;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.FOHTypes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        e.IsActive = false;
        e.UpdatedAt = DateTime.UtcNow;
        return await _db.SaveChangesAsync() > 0;
    }
}

public class SaleEnquiryRepository : ISaleEnquiryRepository
{
    private readonly ERPDbContext _db;
    public SaleEnquiryRepository(ERPDbContext db) => _db = db;

    public async Task<SaleEnquiry?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false)
    {
        if (tenantId == Guid.Empty) return null;
        var q = _db.SaleEnquiries.AsQueryable();
        if (includeLines) q = q.Include(x => x.Lines);
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<SaleEnquiry>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<SaleEnquiry>();
        return await _db.SaleEnquiries.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.EnquiryDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SaleEnquiry entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        foreach (var line in entity.Lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = entity.TenantId;
            line.SaleEnquiryId = entity.Id;
            line.CreatedAt = DateTime.UtcNow;
        }
        _db.SaleEnquiries.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(SaleEnquiry entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.SaleEnquiries.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.CustomerId = entity.CustomerId;
        existing.EnquiryDate = entity.EnquiryDate;
        existing.ReferenceNo = entity.ReferenceNo;
        existing.Notes = entity.Notes;
        existing.Status = entity.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.SaleEnquiries.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.SaleEnquiries.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task ReplaceLinesAsync(Guid enquiryId, Guid tenantId, IEnumerable<SaleEnquiryLine> lines)
    {
        var existing = await _db.SaleEnquiryLines
            .Where(x => x.SaleEnquiryId == enquiryId && x.TenantId == tenantId).ToListAsync();
        _db.SaleEnquiryLines.RemoveRange(existing);
        foreach (var line in lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = tenantId;
            line.SaleEnquiryId = enquiryId;
            line.CreatedAt = DateTime.UtcNow;
            _db.SaleEnquiryLines.Add(line);
        }
        await _db.SaveChangesAsync();
    }
}

public class SalePreCostRepository : ISalePreCostRepository
{
    private readonly ERPDbContext _db;
    public SalePreCostRepository(ERPDbContext db) => _db = db;

    public async Task<SalePreCost?> GetByIdAsync(Guid id, Guid tenantId, bool includeLines = false)
    {
        if (tenantId == Guid.Empty) return null;
        var q = _db.SalePreCosts.AsQueryable();
        if (includeLines) q = q.Include(x => x.Lines);
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<SalePreCost>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<SalePreCost>();
        return await _db.SalePreCosts.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PreCostDate)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalePreCost entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        foreach (var line in entity.Lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = entity.TenantId;
            line.SalePreCostId = entity.Id;
            line.CreatedAt = DateTime.UtcNow;
        }
        _db.SalePreCosts.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(SalePreCost entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.SalePreCosts.FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        existing.SaleEnquiryId = entity.SaleEnquiryId;
        existing.QuoteId = entity.QuoteId;
        existing.CustomerId = entity.CustomerId;
        existing.PreCostDate = entity.PreCostDate;
        existing.Notes = entity.Notes;
        existing.Status = entity.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.SalePreCosts.Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.SalePreCosts.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task ReplaceLinesAsync(Guid preCostId, Guid tenantId, IEnumerable<SalePreCostLine> lines)
    {
        var existing = await _db.SalePreCostLines
            .Where(x => x.SalePreCostId == preCostId && x.TenantId == tenantId).ToListAsync();
        _db.SalePreCostLines.RemoveRange(existing);
        foreach (var line in lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TenantId = tenantId;
            line.SalePreCostId = preCostId;
            line.CreatedAt = DateTime.UtcNow;
            _db.SalePreCostLines.Add(line);
        }
        await _db.SaveChangesAsync();
    }
}
