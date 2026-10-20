using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

/// <summary>
/// EF-backed warehouse store so AuditSaveChangesInterceptor and TenantGuardInterceptor apply on writes.
/// </summary>
public class WarehouseRepository : IWarehouseRepository
{
    private readonly ERPDbContext _db;

    public WarehouseRepository(ERPDbContext db) => _db = db;

    private IQueryable<Warehouse> Query(Guid tenantId) =>
        _db.Warehouses.Where(w => w.TenantId == tenantId);

    public async Task<Warehouse?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(Guid tenantId, Guid companyId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Warehouse>();
        var q = Query(tenantId);
        if (companyId != Guid.Empty)
            q = q.Where(w => w.CompanyId == companyId);
        return await q.OrderBy(w => w.WarehouseName).ToListAsync();
    }

    public async Task<Warehouse> AddAsync(Warehouse warehouse)
    {
        if (warehouse.Id == Guid.Empty) warehouse.Id = Guid.NewGuid();
        if (warehouse.CreatedAt == default) warehouse.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(warehouse.Name))
            warehouse.Name = warehouse.WarehouseName;
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
    {
        warehouse.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(warehouse.Name))
            warehouse.Name = warehouse.WarehouseName;
        _db.Warehouses.Update(warehouse);
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.Warehouses.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId) =>
        tenantId != Guid.Empty && await Query(tenantId).AnyAsync(w => w.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(code)) return false;
        var q = Query(tenantId).Where(w => w.CompanyId == companyId && w.WarehouseCode == code);
        if (excludeId.HasValue && excludeId != Guid.Empty)
            q = q.Where(w => w.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}
