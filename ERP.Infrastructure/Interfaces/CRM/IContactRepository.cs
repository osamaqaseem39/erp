using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(int id);
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<Contact> AddAsync(Contact entity);
    Task UpdateAsync(Contact entity);
    Task DeleteAsync(int id);
}