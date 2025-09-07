using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class JournalMapper
{
    public static JournalDto ToDto(Journal entity)
    {
        return new JournalDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            JournalId = entity.JournalId,
            JournalType = entity.JournalType,
            ReferenceNo = entity.ReferenceNo,
            JournalDate = entity.JournalDate,
            Description = entity.Description,
            CurrencyCode = entity.CurrencyCode,
            ExchangeRate = entity.ExchangeRate,
            TotalDebit = entity.TotalDebit,
            TotalCredit = entity.TotalCredit,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            CreatedBy = Guid.TryParse(entity.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            PostedAt = entity.PostedAt,
            PostedBy = entity.PostedBy,
            IsLocked = entity.IsLocked,
            JournalEntries = entity.JournalEntries?.Select(JournalEntryMapper.ToDto).ToList()
        };
    }

    public static Journal ToEntity(CreateJournalDto dto, Guid tenantId, string createdBy)
    {
        return new Journal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            JournalId = Guid.NewGuid(),
            JournalType = dto.JournalType,
            ReferenceNo = dto.ReferenceNo,
            JournalDate = dto.JournalDate,
            Description = dto.Description,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            Status = dto.Status,
            IsLocked = dto.IsLocked,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateJournalDto dto, Journal entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.JournalType))
            entity.JournalType = dto.JournalType;
        
        entity.ReferenceNo = dto.ReferenceNo;
        
        if (dto.JournalDate.HasValue)
            entity.JournalDate = dto.JournalDate.Value;
        
        entity.Description = dto.Description;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.ExchangeRate = dto.ExchangeRate;
        
        if (!string.IsNullOrEmpty(dto.Status))
            entity.Status = dto.Status;
        
        if (dto.IsLocked.HasValue)
            entity.IsLocked = dto.IsLocked.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}

public static class JournalEntryMapper
{
    public static JournalEntryDto ToDto(JournalEntry entity)
    {
        return new JournalEntryDto
        {
            Id = entity.EntryId,
            TenantId = entity.TenantId,
            EntryId = entity.EntryId,
            JournalId = entity.JournalId,
            AccountId = entity.AccountId,
            AccountCode = string.Empty, // TODO: Get from Account navigation
            AccountName = string.Empty, // TODO: Get from Account navigation
            CostCenterId = entity.CostCenterId,
            CostCenterName = entity.CostCenter?.CostCenterName,
            ProjectId = entity.ProjectId,
            Debit = entity.Debit,
            Credit = entity.Credit,
            Narration = entity.Narration,
            EntityReferenceTable = entity.EntityReferenceTable,
            EntityReferenceId = entity.EntityReferenceId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy?.ToString()
        };
    }

    public static JournalEntry ToEntity(CreateJournalEntryDto dto, Guid tenantId, string createdBy)
    {
        return new JournalEntry
        {
            EntryId = Guid.NewGuid(),
            JournalId = dto.JournalId,
            TenantId = tenantId,
            AccountId = dto.AccountId,
            CostCenterId = dto.CostCenterId,
            ProjectId = dto.ProjectId,
            Debit = dto.Debit,
            Credit = dto.Credit,
            Narration = dto.Narration,
            EntityReferenceTable = dto.EntityReferenceTable,
            EntityReferenceId = dto.EntityReferenceId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Parse(createdBy)
        };
    }

    public static void UpdateEntity(UpdateJournalEntryDto dto, JournalEntry entity, string updatedBy)
    {
        if (dto.AccountId.HasValue)
            entity.AccountId = dto.AccountId.Value;
        
        // AccountCode and AccountName are not stored in entity, only in DTO
        
        entity.CostCenterId = dto.CostCenterId;
        entity.ProjectId = dto.ProjectId;
        
        if (dto.Debit.HasValue)
            entity.Debit = dto.Debit.Value;
        
        if (dto.Credit.HasValue)
            entity.Credit = dto.Credit.Value;
        
        entity.Narration = dto.Narration;
        entity.EntityReferenceTable = dto.EntityReferenceTable;
        entity.EntityReferenceId = dto.EntityReferenceId;
    }
}