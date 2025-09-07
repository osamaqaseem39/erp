using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces;
using ERP.Infrastructure.Interfaces.Finance;
using ERP.Domain.Finance;

namespace ERP.Application.Finance;

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IChartOfAccountRepository _repository;

    public ChartOfAccountsService(IChartOfAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChartOfAccountDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateChartOfAccountDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateChartOfAccountDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetByTypeAsync(string accountType)
    {
        var entities = await _repository.GetByTypeAsync(accountType);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetByParentAsync(Guid parentAccountId)
    {
        var entities = await _repository.GetByParentAsync(parentAccountId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetActiveAccountsAsync()
    {
        var entities = await _repository.GetActiveAccountsAsync();
        return entities.Select(MapToDto);
    }

    private static ChartOfAccountDto MapToDto(ChartOfAccount entity)
    {
        return new ChartOfAccountDto
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            TenantId = entity.TenantId,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            ParentAccountId = entity.ParentAccountId,
            CurrencyCode = entity.CurrencyCode,
            IsActive = entity.IsActive,
            IsPostingAllowed = entity.IsPostingAllowed,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = !string.IsNullOrEmpty(entity.CreatedBy) ? Guid.Parse(entity.CreatedBy) : null,
            UpdatedBy = !string.IsNullOrEmpty(entity.UpdatedBy) ? Guid.Parse(entity.UpdatedBy) : null
        };
    }

    private static ChartOfAccount MapToEntity(CreateChartOfAccountDto dto)
    {
        return new ChartOfAccount
        {
            TenantId = dto.TenantId,
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            AccountType = dto.AccountType,
            ParentAccountId = dto.ParentAccountId,
            CurrencyCode = dto.CurrencyCode,
            IsActive = dto.IsActive,
            IsPostingAllowed = dto.IsPostingAllowed,
            CreatedBy = dto.CreatedBy?.ToString()
        };
    }

    private static void MapToEntity(UpdateChartOfAccountDto dto, ChartOfAccount entity)
    {
        if (dto.AccountCode != null) entity.AccountCode = dto.AccountCode;
        if (dto.AccountName != null) entity.AccountName = dto.AccountName;
        if (dto.AccountType != null) entity.AccountType = dto.AccountType;
        entity.ParentAccountId = dto.ParentAccountId;
        entity.CurrencyCode = dto.CurrencyCode;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        if (dto.IsPostingAllowed.HasValue) entity.IsPostingAllowed = dto.IsPostingAllowed.Value;
        entity.UpdatedBy = dto.UpdatedBy?.ToString();
    }
}