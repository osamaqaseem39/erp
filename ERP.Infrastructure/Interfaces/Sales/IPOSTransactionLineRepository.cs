using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSTransactionLineRepository
{
    Task<IEnumerable<POSTransactionLine>> GetByTransactionIdAsync(Guid transactionId, Guid tenantId);
    Task<Guid> CreateAsync(POSTransactionLine line);
    Task<bool> DeleteByTransactionIdAsync(Guid transactionId, Guid tenantId);
}
