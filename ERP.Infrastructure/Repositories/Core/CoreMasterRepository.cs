using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class CoreMasterRepository : ICoreMasterRepository
{
    private readonly ERPDbContext _db;
    public CoreMasterRepository(ERPDbContext db) => _db = db;

    public Task<Country?> GetCountryAsync(Guid id, Guid tenantId) =>
        _db.Countries.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

    public async Task<IReadOnlyList<Country>> ListCountriesAsync(Guid tenantId, bool? isActive = null)
    {
        var q = _db.Countries.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(e => e.IsActive == isActive.Value);
        return await q.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Guid> AddCountryAsync(Country entity)
    {
        _db.Countries.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateCountryAsync(Country entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Countries.Update(entity);
        await _db.SaveChangesAsync();
    }

    public Task<City?> GetCityAsync(Guid id, Guid tenantId) =>
        _db.Cities.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

    public async Task<IReadOnlyList<City>> ListCitiesAsync(Guid tenantId, Guid? countryId = null, bool? isActive = null)
    {
        var q = _db.Cities.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (countryId.HasValue) q = q.Where(e => e.CountryId == countryId.Value);
        if (isActive.HasValue) q = q.Where(e => e.IsActive == isActive.Value);
        return await q.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Guid> AddCityAsync(City entity)
    {
        _db.Cities.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateCityAsync(City entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Cities.Update(entity);
        await _db.SaveChangesAsync();
    }

    public Task<Branch?> GetBranchAsync(Guid id, Guid tenantId) =>
        _db.Branches.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

    public async Task<IReadOnlyList<Branch>> ListBranchesAsync(Guid tenantId, bool? isActive = null)
    {
        var q = _db.Branches.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(e => e.IsActive == isActive.Value);
        return await q.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Guid> AddBranchAsync(Branch entity)
    {
        _db.Branches.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateBranchAsync(Branch entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Branches.Update(entity);
        await _db.SaveChangesAsync();
    }

    public Task<PaymentTerm?> GetPaymentTermAsync(Guid id, Guid tenantId) =>
        _db.PaymentTerms.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

    public async Task<IReadOnlyList<PaymentTerm>> ListPaymentTermsAsync(Guid tenantId, bool? isActive = null)
    {
        var q = _db.PaymentTerms.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(e => e.IsActive == isActive.Value);
        return await q.OrderBy(e => e.Days).ThenBy(e => e.Name).ToListAsync();
    }

    public async Task<Guid> AddPaymentTermAsync(PaymentTerm entity)
    {
        _db.PaymentTerms.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdatePaymentTermAsync(PaymentTerm entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.PaymentTerms.Update(entity);
        await _db.SaveChangesAsync();
    }
}
