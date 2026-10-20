using ERP.Domain.Finance;
using Contracts = ERP.Infrastructure.Interfaces.Finance;
using Repos = ERP.Infrastructure.Repositories.Finance;

namespace ERP.Infrastructure.Repositories.Adapters;

/// <summary>
/// Bridges the Interfaces.Finance chart-of-accounts contract onto the existing repository.
/// </summary>
public class ChartOfAccountRepositoryAdapter : Contracts.IChartOfAccountRepository
{
    private readonly Repos.IChartOfAccountRepository _inner;

    public ChartOfAccountRepositoryAdapter(Repos.IChartOfAccountRepository inner) => _inner = inner;

    public Task<ChartOfAccount?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<IEnumerable<ChartOfAccount>> GetAllAsync() => _inner.GetAllAsync();
    public Task<Guid> CreateAsync(ChartOfAccount account) => _inner.CreateAsync(account);
    public Task<bool> UpdateAsync(ChartOfAccount account) => _inner.UpdateAsync(account);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public async Task<bool> ExistsAsync(Guid id) => await _inner.GetByIdAsync(id) != null;
    public Task<ChartOfAccount?> GetByCodeAsync(string code) => _inner.GetByCodeAsync(code);

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var existing = await _inner.GetByCodeAsync(code);
        return existing == null || (excludeId.HasValue && existing.AccountId == excludeId.Value);
    }

    public Task<IEnumerable<ChartOfAccount>> GetByTypeAsync(string accountType) => _inner.GetByTypeAsync(accountType);
    public Task<IEnumerable<ChartOfAccount>> GetByParentAsync(Guid? parentAccountId) => _inner.GetByParentAsync(parentAccountId);
    public Task<IEnumerable<ChartOfAccount>> GetActiveAccountsAsync() => _inner.GetActiveAsync();
}

/// <summary>
/// Bridges the Interfaces.Finance payment contract onto the existing repository.
/// </summary>
public class PaymentRepositoryAdapter : Contracts.IPaymentRepository
{
    private readonly Repos.IPaymentRepository _inner;

    public PaymentRepositoryAdapter(Repos.IPaymentRepository inner) => _inner = inner;

    public Task<Payment?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
    public Task<Payment?> GetByIdAsync(Guid id, Guid tenantId) => _inner.GetByIdAsync(id, tenantId);
    public Task<IEnumerable<Payment>> GetAllAsync() => _inner.GetAllAsync();
    public Task<IEnumerable<Payment>> GetAllAsync(Guid tenantId) => _inner.GetAllAsync(tenantId);
    public Task<Guid> CreateAsync(Payment payment) => _inner.CreateAsync(payment);
    public Task<bool> UpdateAsync(Payment payment) => _inner.UpdateAsync(payment);
    public Task<bool> DeleteAsync(Guid id) => _inner.DeleteAsync(id);
    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _inner.DeleteAsync(id, tenantId);

    public async Task<bool> ExistsAsync(Guid id) => await _inner.GetByIdAsync(id) != null;
    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
        => await _inner.GetByIdAsync(id, tenantId) != null;

    public Task<Payment?> GetByNumberAsync(string number) => _inner.GetByNumberAsync(number);
    public Task<Payment?> GetByNumberAsync(string number, Guid tenantId)
        => _inner.GetByNumberAsync(number, tenantId);

    public async Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null)
    {
        var existing = await _inner.GetByNumberAsync(number);
        return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
    }

    public async Task<bool> IsNumberUniqueAsync(string number, Guid tenantId, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty) return false;
        var existing = await _inner.GetByNumberAsync(number, tenantId);
        return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId)
        => (await _inner.GetAllAsync()).Where(p => p.RelatedInvoiceId == invoiceId);

    public Task<IEnumerable<Payment>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId)
        => _inner.GetByInvoiceAsync(invoiceId, tenantId);

    public async Task<IEnumerable<Payment>> GetByBillAsync(Guid billId)
        => (await _inner.GetAllAsync()).Where(p => p.RelatedBillId == billId);

    public Task<IEnumerable<Payment>> GetByBillAsync(Guid billId, Guid tenantId)
        => _inner.GetByBillAsync(billId, tenantId);

    public Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        => _inner.GetByDateRangeAsync(startDate, endDate);

    public Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
        => _inner.GetByDateRangeAsync(startDate, endDate, tenantId);
}
