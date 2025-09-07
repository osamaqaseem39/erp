using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly ERPDbContext _context;

    public UserSessionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession?> GetByIdAsync(int id)
    {
        return await _context.UserSessions.FindAsync(id);
    }

    public async Task<IEnumerable<UserSession>> GetAllAsync()
    {
        return await _context.UserSessions.ToListAsync();
    }

    public async Task<UserSession> AddAsync(UserSession entity)
    {
        _context.UserSessions.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserSession entity)
    {
        _context.UserSessions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.UserSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}