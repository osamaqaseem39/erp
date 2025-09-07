using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IContactService
{
    Task<ContactDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ContactDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateContactDto dto);
    Task<bool> UpdateAsync(UpdateContactDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ContactDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<ContactDto>> SearchAsync(string searchTerm);
}