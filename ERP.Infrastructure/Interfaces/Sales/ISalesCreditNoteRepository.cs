using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface ISalesCreditNoteRepository
{
    Task<SalesCreditNote?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesCreditNote>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<SalesCreditNote>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesCreditNote>> GetByStatusAsync(string status, Guid tenantId);
    Task<SalesCreditNote> AddAsync(SalesCreditNote entity);
    Task UpdateAsync(SalesCreditNote entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

