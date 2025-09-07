using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IPayrollComponentRepository
{
    Task<PayrollComponent?> GetByIdAsync(Guid id);
    Task<IEnumerable<PayrollComponent>> GetAllAsync();
    Task<IEnumerable<PayrollComponent>> GetActiveAsync();
    Task<IEnumerable<PayrollComponent>> GetByTypeAsync(string type);
    Task<Guid> CreateAsync(PayrollComponent payrollComponent);
    Task<bool> UpdateAsync(PayrollComponent payrollComponent);
    Task<bool> DeleteAsync(Guid id);
}