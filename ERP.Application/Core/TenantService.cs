using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Core;

namespace ERP.Application.Core;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _repository;

    public TenantService(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        var tenant = await _repository.GetByIdAsync(id);
        return tenant != null ? MapToDto(tenant) : null;
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _repository.GetAllAsync();
        return tenants.Select(MapToDto);
    }

    public async Task<PagedResult<TenantDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("GetPagedAsync method needs to be implemented in the repository");
    }

    public async Task<TenantDto?> GetBySubdomainAsync(string subdomain)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("GetBySubdomainAsync method needs to be implemented in the repository");
    }

    public async Task<Guid> CreateAsync(CreateTenantDto dto)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            TenantName = dto.Name,
            Domain = dto.Domain ?? string.Empty,
            Industry = dto.Industry ?? string.Empty,
            Country = dto.Country ?? string.Empty,
            Status = dto.Status,
            DatabaseName = dto.DatabaseName ?? $"ERP_{dto.Domain}",
            ConnectionString = $"Server=localhost;Database=ERP_{dto.Domain};Trusted_Connection=true;TrustServerCertificate=true;",
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };

        return await _repository.CreateAsync(tenant);
    }

    public async Task<bool> UpdateAsync(UpdateTenantDto dto)
    {
        var tenant = await _repository.GetByIdAsync(dto.Id);
        if (tenant == null) return false;

        tenant.TenantName = dto.Name;
        tenant.Domain = dto.Domain ?? string.Empty;
        tenant.Industry = dto.Industry ?? string.Empty;
        tenant.Country = dto.Country ?? string.Empty;
        tenant.Status = dto.Status;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.UpdatedBy = dto.UpdatedBy;

        return await _repository.UpdateAsync(tenant);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        // This method needs to be implemented in the repository
        throw new NotImplementedException("ExistsAsync method needs to be implemented in the repository");
    }

    private static TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.TenantName,
            Domain = tenant.Domain,
            Industry = tenant.Industry,
            Country = tenant.Country,
            Status = tenant.Status,
            DatabaseName = tenant.DatabaseName,
            ConnectionString = tenant.ConnectionString,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            CreatedBy = tenant.CreatedBy,
            UpdatedBy = tenant.UpdatedBy
        };
    }
}