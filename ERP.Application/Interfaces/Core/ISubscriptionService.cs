using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface ISubscriptionService
{
    Task<SubscriptionDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionDto>> GetAllAsync();
    Task<IEnumerable<SubscriptionDto>> GetByTenantAsync(Guid tenantId);
    Task<IEnumerable<SubscriptionDto>> GetByStatusAsync(string status);
    Task<SubscriptionDto?> GetActiveByTenantAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSubscriptionDto dto);
    Task<bool> UpdateAsync(UpdateSubscriptionDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> CancelAsync(Guid id);
    Task<bool> RenewAsync(Guid id);
}