using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IPayrollService
{
    Task<PayrollDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PayrollDto>> GetAllAsync();
    Task<Guid> CreateAsync(PayrollDto dto);
    Task<bool> UpdateAsync(PayrollDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<PayrollDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
    Task<bool> ProcessPayrollAsync(Guid payrollId);
}