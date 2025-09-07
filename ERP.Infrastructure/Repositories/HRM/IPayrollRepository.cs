using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IPayrollRepository
{
    Task<Payroll?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payroll>> GetAllAsync();
    Task<IEnumerable<Payroll>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payroll>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(Payroll payroll);
    Task<bool> UpdateAsync(Payroll payroll);
    Task<bool> DeleteAsync(Guid id);
}