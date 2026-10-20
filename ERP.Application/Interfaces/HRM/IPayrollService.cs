using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IPayrollService
{
    Task<PayrollDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PayrollDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(PayrollDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(PayrollDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<PayrollDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    /// <summary>Draft → generate lines + Processed; Processed → Posted (immutable).</summary>
    Task<bool> ProcessPayrollAsync(Guid payrollId, Guid tenantId, Guid userId);
}
