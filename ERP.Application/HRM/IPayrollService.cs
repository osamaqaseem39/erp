using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public interface IPayrollService
{
    Task<PayrollResultDto> RunPayrollAsync(PayrollRunDto dto);
    Task<PayrollDto> GetPayrollByIdAsync(Guid payrollId);
    Task<IEnumerable<PayrollDto>> GetPayrollsByCompanyAsync(Guid companyId);
    Task<bool> PostPayrollAsync(Guid payrollId, Guid postedBy);
    Task<bool> DeletePayrollAsync(Guid payrollId);
    Task<IEnumerable<PayrollLineDto>> GetPayrollLinesAsync(Guid payrollId);
}