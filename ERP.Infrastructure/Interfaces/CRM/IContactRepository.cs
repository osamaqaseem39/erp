using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Contact>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Contact>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<Contact>> SearchAsync(string searchTerm, Guid tenantId);
    Task<Guid> CreateAsync(Contact contact);
    Task<bool> UpdateAsync(Contact contact);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
