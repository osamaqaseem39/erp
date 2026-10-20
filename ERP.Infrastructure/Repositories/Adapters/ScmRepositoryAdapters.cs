using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Contracts = ERP.Infrastructure.Interfaces.SCM;

namespace ERP.Infrastructure.Repositories.Adapters;

/// <summary>
/// Implements the non-tenant Interfaces.SCM warehouse contract against EF Core, scoped by <see cref="ITenantProvider"/>.
/// </summary>
public class WarehouseRepositoryAdapter : Contracts.IWarehouseRepository
{
    private readonly ERPDbContext _db;
    private readonly ITenantProvider _tenant;

    public WarehouseRepositoryAdapter(ERPDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    private Guid? TenantId => _tenant.GetTenantId();

    private IQueryable<Warehouse> Query()
    {
        var tid = TenantId;
        if (!tid.HasValue || tid == Guid.Empty)
            return _db.Warehouses.Where(_ => false);
        return _db.Warehouses.Where(w => w.TenantId == tid.Value);
    }

    public Task<Warehouse?> GetByIdAsync(Guid id)
        => Query().FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
        => await Query().OrderBy(w => w.WarehouseName).ToListAsync();

    public async Task<Warehouse> AddAsync(Warehouse entity)
    {
        var tid = TenantId;
        if (tid.HasValue && tid != Guid.Empty && entity.TenantId == Guid.Empty)
            entity.TenantId = tid.Value;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(entity.Name))
            entity.Name = entity.WarehouseName;
        _db.Warehouses.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Warehouse entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(entity.Name))
            entity.Name = entity.WarehouseName;
        _db.Warehouses.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return;
        _db.Warehouses.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id)
        => Query().AnyAsync(w => w.Id == id);

    public async Task<IEnumerable<Warehouse>> GetByCompanyIdAsync(Guid companyId)
        => await Query().Where(w => w.CompanyId == companyId).OrderBy(w => w.WarehouseName).ToListAsync();

    public Task<Warehouse?> GetByCodeAsync(string code)
        => Query().FirstOrDefaultAsync(w => w.WarehouseCode == code);
}
