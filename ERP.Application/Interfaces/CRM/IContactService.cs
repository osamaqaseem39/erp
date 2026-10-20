using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IContactService
{
    Task<ContactDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ContactDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateContactDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateContactDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeactivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<IEnumerable<ContactDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<ContactDto>> SearchAsync(string searchTerm, Guid tenantId);
}
