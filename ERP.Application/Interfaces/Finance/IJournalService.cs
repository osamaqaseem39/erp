using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IJournalService
{
    Task<JournalEntryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<JournalEntryDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateJournalEntryDto dto);
    Task<bool> UpdateAsync(UpdateJournalEntryDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<JournalEntryDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<JournalEntryDto>> GetByReferenceAsync(string reference);
    Task<bool> PostAsync(Guid id);
    Task<bool> ReverseAsync(Guid id);
}