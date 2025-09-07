using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class ContactRepository : IContactRepository
{
    private readonly ERPDbContext _context;

    public ContactRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        return await _context.Contacts.FindAsync(id);
    }

    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        return await _context.Contacts.ToListAsync();
    }

    public async Task<Contact> AddAsync(Contact entity)
    {
        _context.Contacts.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Contact entity)
    {
        _context.Contacts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Contacts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}