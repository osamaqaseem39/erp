using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id);
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<IEnumerable<Contact>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Contact>> SearchAsync(string searchTerm);
    Task<Guid> CreateAsync(Contact contact);
    Task<bool> UpdateAsync(Contact contact);
    Task<bool> DeleteAsync(Guid id);
}
