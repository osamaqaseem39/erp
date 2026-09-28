using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

/// <summary>Adapter so PayrollController (Interfaces.HRM.IPayrollService) resolves.</summary>
public class PayrollCrudService : ERP.Application.Interfaces.HRM.IPayrollService
{
    public Task<PayrollDto?> GetByIdAsync(Guid id) => Task.FromResult<PayrollDto?>(null);
    public Task<IEnumerable<PayrollDto>> GetAllAsync() => Task.FromResult(Enumerable.Empty<PayrollDto>());
    public Task<Guid> CreateAsync(PayrollDto dto) => Task.FromResult(Guid.Empty);
    public Task<bool> UpdateAsync(PayrollDto dto) => Task.FromResult(false);
    public Task<bool> DeleteAsync(Guid id) => Task.FromResult(false);
    public Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(Guid employeeId) => Task.FromResult(Enumerable.Empty<PayrollDto>());
    public Task<IEnumerable<PayrollDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate) => Task.FromResult(Enumerable.Empty<PayrollDto>());
    public Task<bool> ProcessPayrollAsync(Guid payrollId) => Task.FromResult(false);
}
