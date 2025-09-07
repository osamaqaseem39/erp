using ERP.Shared.DTOs.Core;

namespace ERP.Api.Services;

public interface ITenantApiService : IApiService<TenantDto, CreateTenantDto, UpdateTenantDto>
{
    Task<TenantDto?> GetByDomainAsync(string domain);
}