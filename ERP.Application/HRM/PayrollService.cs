using ERP.Application.Interfaces;
using ERP.Shared.DTOs.HRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _repository;

    public PayrollService(IPayrollRepository repository)
    {
        _repository = repository;
    }

    public async Task<PayrollResultDto> RunPayrollAsync(PayrollRunDto dto)
    {
        // Implementation for running payroll
        var payrollId = Guid.NewGuid();
        var payroll = new Payroll
        {
            Id = payrollId,
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            PayPeriodStart = dto.PayPeriodStart,
            PayPeriodEnd = dto.PayPeriodEnd,
            PayDate = dto.PayDate,
            Status = "Draft",
            CreatedBy = dto.CreatedBy
        };

        await _repository.CreateAsync(payroll);

        return new PayrollResultDto
        {
            PayrollId = payrollId,
            Status = "Draft",
            TotalEmployees = 0,
            TotalGrossPay = 0,
            TotalDeductions = 0,
            TotalNetPay = 0
        };
    }

    public async Task<PayrollDto> GetPayrollByIdAsync(Guid payrollId)
    {
        var entity = await _repository.GetByIdAsync(payrollId);
        if (entity == null) throw new KeyNotFoundException($"Payroll with ID '{payrollId}' not found.");
        return MapToDto(entity);
    }

    public async Task<IEnumerable<PayrollDto>> GetPayrollsByCompanyAsync(Guid companyId)
    {
        var entities = await _repository.GetByCompanyAsync(companyId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> PostPayrollAsync(Guid payrollId, Guid postedBy)
    {
        var entity = await _repository.GetByIdAsync(payrollId);
        if (entity == null) return false;

        entity.Status = "Posted";
        entity.PostedBy = postedBy;
        entity.PostedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeletePayrollAsync(Guid payrollId)
    {
        return await _repository.DeleteAsync(payrollId);
    }

    public async Task<IEnumerable<PayrollLineDto>> GetPayrollLinesAsync(Guid payrollId)
    {
        var payroll = await _repository.GetByIdAsync(payrollId);
        if (payroll == null) throw new KeyNotFoundException($"Payroll with ID '{payrollId}' not found.");

        // Return empty list for now - this would be populated from payroll lines
        return new List<PayrollLineDto>();
    }

    private static PayrollDto MapToDto(Payroll entity)
    {
        return new PayrollDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            EmployeeId = entity.EmployeeId,
            PayPeriodStart = entity.PayPeriodStart,
            PayPeriodEnd = entity.PayPeriodEnd,
            PayDate = entity.PayDate,
            BasicSalary = entity.BasicSalary,
            Allowances = entity.Allowances,
            Deductions = entity.Deductions,
            GrossSalary = entity.GrossSalary,
            NetSalary = entity.NetSalary,
            Status = entity.Status,
            ProcessedAt = entity.ProcessedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

}