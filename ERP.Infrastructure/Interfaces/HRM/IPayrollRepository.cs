using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IPayrollRepository
{
    Task<Payroll?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payroll>> GetAllAsync();
    Task<Guid> CreateAsync(Payroll payroll);
    Task<bool> UpdateAsync(Payroll payroll);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Payroll>> GetByCompanyAsync(Guid companyId);
    Task<IEnumerable<Payroll>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
}