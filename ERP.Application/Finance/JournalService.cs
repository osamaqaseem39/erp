using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.Finance;
using ERP.Domain.Finance;

namespace ERP.Application.Finance;

public class JournalService : IJournalService
{
    private readonly IJournalEntryRepository _repository;

    public JournalService(IJournalEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<JournalEntryDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<JournalEntryDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateJournalEntryDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateJournalEntryDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> PostAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        // Note: Status and PostedDate properties don't exist in the current JournalEntry domain entity
        // This would need to be implemented in the domain entity first
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> ReverseAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        // Note: Status and ReversedDate properties don't exist in the current JournalEntry domain entity
        // This would need to be implemented in the domain entity first
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<IEnumerable<JournalEntryDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetAllAsync();
        return entities.Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate).Select(MapToDto);
    }

    public async Task<IEnumerable<JournalEntryDto>> GetByReferenceAsync(string reference)
    {
        var entities = await _repository.GetAllAsync();
        return entities.Where(e => e.EntityReferenceTable == reference).Select(MapToDto);
    }

    private static JournalEntryDto MapToDto(JournalEntry entity)
    {
        return new JournalEntryDto
        {
            EntryId = entity.EntryId,
            JournalId = entity.JournalId,
            AccountId = entity.AccountId,
            CostCenterId = entity.CostCenterId,
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

    private static JournalEntry MapToEntity(CreateJournalEntryDto dto)
    {
        return new JournalEntry
        {
            EntryId = Guid.NewGuid(),
            JournalId = dto.JournalId,
            TenantId = dto.TenantId,
            AccountId = dto.AccountId,
            CostCenterId = dto.CostCenterId,
            ProjectId = dto.ProjectId,
            Debit = dto.Debit,
            Credit = dto.Credit,
            Narration = dto.Narration,
            EntityReferenceTable = dto.EntityReferenceTable,
            EntityReferenceId = dto.EntityReferenceId,
            CreatedBy = !string.IsNullOrEmpty(dto.CreatedBy) ? Guid.Parse(dto.CreatedBy) : null
        };
    }

    private static void MapToEntity(UpdateJournalEntryDto dto, JournalEntry entity)
    {
        entity.AccountId = dto.AccountId ?? entity.AccountId;
        entity.CostCenterId = dto.CostCenterId ?? entity.CostCenterId;
        entity.ProjectId = dto.ProjectId ?? entity.ProjectId;
        entity.Debit = dto.Debit ?? entity.Debit;
        entity.Credit = dto.Credit ?? entity.Credit;
        entity.Narration = dto.Narration ?? entity.Narration;
        entity.EntityReferenceTable = dto.EntityReferenceTable ?? entity.EntityReferenceTable;
        entity.EntityReferenceId = dto.EntityReferenceId ?? entity.EntityReferenceId;
    }
}