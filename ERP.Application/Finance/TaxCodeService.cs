using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Finance;

public class TaxCodeService : ITaxCodeService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sales", "Purchase", "VAT", "GST", "Input", "Output"
    };

    private readonly ITaxCodeRepository _repository;
    private readonly ERP.Application.Interfaces.Finance.IChartOfAccountRepository _accounts;

    public TaxCodeService(
        ITaxCodeRepository repository,
        ERP.Application.Interfaces.Finance.IChartOfAccountRepository accounts)
    {
        _repository = repository;
        _accounts = accounts;
    }

    public async Task<TaxCodeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<TaxCodeDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCodeDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<TaxCodeDto>> GetActiveAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCodeDto>();
        return (await _repository.GetActiveAsync(tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<TaxCodeDto>> GetByTypeAsync(string taxType, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxCodeDto>();
        return (await _repository.GetByTypeAsync(taxType, tenantId)).Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateTaxCodeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (string.IsNullOrWhiteSpace(dto.TaxCode) || string.IsNullOrWhiteSpace(dto.TaxName))
            return Guid.Empty;
        if (!dto.GLAccountId.HasValue || dto.GLAccountId == Guid.Empty)
            return Guid.Empty;
        if (!await IsValidPostingAccountAsync(dto.GLAccountId.Value, tenantId))
            return Guid.Empty;

        var code = dto.TaxCode.Trim();
        if (await _repository.GetByCodeAsync(code, tenantId) != null)
            return Guid.Empty;

        var rate = dto.TaxRate ?? 0;
        if (rate < 0) return Guid.Empty;

        var taxType = string.IsNullOrWhiteSpace(dto.TaxType) ? "Sales" : dto.TaxType.Trim();
        if (!AllowedTypes.Contains(taxType)) return Guid.Empty;

        var id = Guid.NewGuid();
        var entity = new TaxCode
        {
            Id = id,
            TaxCodeId = id,
            TenantId = tenantId,
            Code = code,
            TaxCodeName = dto.TaxName.Trim(),
            TaxCodeDescription = dto.Description?.Trim() ?? string.Empty,
            TaxRate = rate,
            TaxType = taxType,
            AccountId = dto.GLAccountId.Value,
            IsActive = dto.IsActive ?? true,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateTaxCodeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        if (dto.TaxCode != null)
        {
            var code = dto.TaxCode.Trim();
            if (string.IsNullOrWhiteSpace(code)) return false;
            var existing = await _repository.GetByCodeAsync(code, tenantId);
            if (existing != null && existing.Id != entity.Id) return false;
            entity.Code = code;
        }

        if (dto.TaxName != null)
        {
            if (string.IsNullOrWhiteSpace(dto.TaxName)) return false;
            entity.TaxCodeName = dto.TaxName.Trim();
        }

        if (dto.Description != null)
            entity.TaxCodeDescription = dto.Description.Trim();

        if (dto.TaxType != null)
        {
            var taxType = dto.TaxType.Trim();
            if (!AllowedTypes.Contains(taxType)) return false;
            entity.TaxType = taxType;
        }

        if (dto.TaxRate.HasValue)
        {
            if (dto.TaxRate.Value < 0) return false;
            entity.TaxRate = dto.TaxRate.Value;
        }

        if (dto.GLAccountId.HasValue)
        {
            if (dto.GLAccountId == Guid.Empty) return false;
            if (!await IsValidPostingAccountAsync(dto.GLAccountId.Value, tenantId))
                return false;
            entity.AccountId = dto.GLAccountId.Value;
        }

        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    private async Task<bool> IsValidPostingAccountAsync(Guid accountId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var account = await _accounts.GetByIdAsync(accountId, tenantId);
        return account != null && account.IsPostingAllowed && account.IsActive;
    }

    private static TaxCodeDto MapToDto(TaxCode entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        TaxCode = entity.Code,
        TaxName = entity.TaxCodeName,
        TaxType = entity.TaxType,
        TaxRate = entity.TaxRate,
        GLAccountId = entity.AccountId == Guid.Empty ? null : entity.AccountId,
        GLAccountName = entity.Account?.AccountName,
        IsActive = entity.IsActive,
        Description = entity.TaxCodeDescription,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy
    };
}
