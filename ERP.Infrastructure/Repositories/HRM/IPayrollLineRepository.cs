using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IPayrollLineRepository
{
    Task<PayrollLine?> GetByIdAsync(Guid id);
    Task<IEnumerable<PayrollLine>> GetAllAsync();
    Task<IEnumerable<PayrollLine>> GetByPayrollAsync(Guid payrollId);
    Task<IEnumerable<PayrollLine>> GetByEmployeeAsync(Guid employeeId);
    Task<Guid> CreateAsync(PayrollLine payrollLine);
    Task<bool> UpdateAsync(PayrollLine payrollLine);
    Task<bool> DeleteAsync(Guid id);
}