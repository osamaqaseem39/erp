using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Finance;

public class JournalRepository : IJournalRepository
{
    private readonly ERPDbContext _context;

    public JournalRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Journal?> GetByIdAsync(Guid id)
    {
        return await _context.Journals
            .Include(j => j.JournalEntries)
            .FirstOrDefaultAsync(j => j.JournalId == id);
    }

    public async Task<IEnumerable<Journal>> GetAllAsync()
    {
        return await _context.Journals
            .Include(j => j.JournalEntries)
            .ToListAsync();
    }

    public async Task<IEnumerable<Journal>> GetByStatusAsync(string status)
    {
        return await _context.Journals
            .Include(j => j.JournalEntries)
            .Where(j => j.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Journal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Journals
            .Include(j => j.JournalEntries)
            .Where(j => j.JournalDate >= startDate && j.JournalDate <= endDate)
            .ToListAsync();
    }

    public async Task<Journal?> GetByNumberAsync(string journalNumber)
    {
        return await _context.Journals
            .Include(j => j.JournalEntries)
            .FirstOrDefaultAsync(j => j.JournalNumber == journalNumber);
    }

    public async Task<Guid> CreateAsync(Journal journal)
    {
        _context.Journals.Add(journal);
        await _context.SaveChangesAsync();
        return journal.JournalId;
    }

    public async Task<bool> UpdateAsync(Journal journal)
    {
        _context.Journals.Update(journal);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var journal = await _context.Journals.FindAsync(id);
        if (journal == null) return false;

        _context.Journals.Remove(journal);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}