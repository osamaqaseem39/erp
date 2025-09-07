using ERP.Application.Core;
using ERP.Shared.DTOs.Core;
using ERP.Api.Mappers;
using ERP.Shared.Exceptions;

namespace ERP.Api.Services;

public class TenantApiService : BaseApiService<TenantDto, CreateTenantDto, UpdateTenantDto>, ITenantApiService
{
    private readonly ITenantService _tenantService;
    private readonly TenantMapper _mapper;

    public TenantApiService(
        ITenantService tenantService,
        TenantMapper mapper,
        ILogger<TenantApiService> logger) : base(logger)
    {
        _tenantService = tenantService;
        _mapper = mapper;
    }

    public override async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        ValidateId(id);
        return await _tenantService.GetByIdAsync(id);
    }

    public override async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        return await _tenantService.GetAllAsync();
    }

    public override async Task<Guid> CreateAsync(CreateTenantDto dto)
    {
        ValidateDto(dto);
        return await _tenantService.CreateAsync(dto);
    }

    public override async Task<bool> UpdateAsync(UpdateTenantDto dto)
    {
        ValidateUpdateDto(dto);
        return await _tenantService.UpdateAsync(dto);
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        ValidateId(id);
        return await _tenantService.DeleteAsync(id);
    }

    public async Task<TenantDto?> GetByDomainAsync(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ValidationException("Domain cannot be empty");

        return await _tenantService.GetBySubdomainAsync(domain);
    }
}