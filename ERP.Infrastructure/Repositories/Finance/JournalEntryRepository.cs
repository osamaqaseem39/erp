using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ERPDbContext _context;

    public JournalEntryRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id)
    {
        return await _context.JournalEntries.FindAsync(id);
    }

    public async Task<IEnumerable<JournalEntry>> GetAllAsync()
    {
        return await _context.JournalEntries.ToListAsync();
    }

    public async Task<Guid> CreateAsync(JournalEntry journalEntry)
    {
        _context.JournalEntries.Add(journalEntry);
        await _context.SaveChangesAsync();
        return journalEntry.Id;
    }

    public async Task<bool> UpdateAsync(JournalEntry journalEntry)
    {
        _context.JournalEntries.Update(journalEntry);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.JournalEntries.Remove(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.JournalEntries.AnyAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.JournalEntries
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<JournalEntry>> GetByReferenceAsync(string reference)
    {
        return await _context.JournalEntries
            .Where(j => j.Reference == reference)
            .ToListAsync();
    }
}