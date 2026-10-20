using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

/// <summary>Simplified payroll: Draft → Process (lines + totals) → Process again (Posted).</summary>
public class PayrollCrudService : ERP.Application.Interfaces.HRM.IPayrollService
{
    /// <summary>Flat tax rate for the simplified gross→net formula.</summary>
    public const decimal DefaultTaxRate = 0.10m;

    private readonly IPayrollRepository _payrolls;
    private readonly IPayrollLineRepository _lines;
    private readonly IEmployeeRepository _employees;

    public PayrollCrudService(
        IPayrollRepository payrolls,
        IPayrollLineRepository lines,
        IEmployeeRepository employees)
    {
        _payrolls = payrolls;
        _lines = lines;
        _employees = employees;
    }

    public async Task<PayrollDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _payrolls.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId || !entity.IsActive) return null;
        return Map(entity);
    }

    public async Task<IEnumerable<PayrollDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PayrollDto>();
        return (await _payrolls.GetAllAsync())
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderByDescending(p => p.PayPeriodEnd)
            .Select(Map);
    }

    public async Task<Guid> CreateAsync(PayrollDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (dto.PayPeriodEnd < dto.PayPeriodStart)
            throw new ArgumentException("Pay period end must be on or after start.");
        if (dto.PayDate == default)
            throw new ArgumentException("Pay date is required.");

        var id = Guid.NewGuid();
        var entity = new Payroll
        {
            Id = id,
            TenantId = tenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            EmployeeId = dto.EmployeeId,
            PayPeriodStart = dto.PayPeriodStart.Date,
            PayPeriodEnd = dto.PayPeriodEnd.Date,
            PayrollPeriodStart = dto.PayPeriodStart.Date,
            PayrollPeriodEnd = dto.PayPeriodEnd.Date,
            PayDate = dto.PayDate.Date,
            PayrollRunDate = DateTime.UtcNow,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency.Trim().ToUpperInvariant(),
            BasicSalary = dto.BasicSalary,
            Allowances = dto.Allowances,
            Deductions = dto.Deductions,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true
        };

        await _payrolls.CreateAsync(entity);
        return id;
    }

    public async Task<bool> UpdateAsync(PayrollDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.Id == Guid.Empty) return false;
        var entity = await RequireDraftAsync(dto.Id, tenantId);

        if (dto.PayPeriodEnd < dto.PayPeriodStart)
            throw new ArgumentException("Pay period end must be on or after start.");

        entity.CompanyId = dto.CompanyId ?? entity.CompanyId;
        entity.EmployeeId = dto.EmployeeId;
        entity.PayPeriodStart = dto.PayPeriodStart.Date;
        entity.PayPeriodEnd = dto.PayPeriodEnd.Date;
        entity.PayrollPeriodStart = dto.PayPeriodStart.Date;
        entity.PayrollPeriodEnd = dto.PayPeriodEnd.Date;
        entity.PayDate = dto.PayDate.Date;
        if (!string.IsNullOrWhiteSpace(dto.Currency))
            entity.Currency = dto.Currency.Trim().ToUpperInvariant();
        entity.BasicSalary = dto.BasicSalary;
        entity.Allowances = dto.Allowances;
        entity.Deductions = dto.Deductions;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();

        return await _payrolls.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _payrolls.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId || !entity.IsActive) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft payroll can be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _payrolls.UpdateAsync(entity);
    }

    public async Task<IEnumerable<PayrollDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PayrollDto>();
        return (await _payrolls.GetByEmployeeAsync(employeeId))
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Select(Map);
    }

    public async Task<IEnumerable<PayrollDto>> GetByPeriodAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PayrollDto>();
        return (await _payrolls.GetByPeriodAsync(startDate.Date, endDate.Date))
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Select(Map);
    }

    public async Task<bool> ProcessPayrollAsync(Guid payrollId, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _payrolls.GetByIdAsync(payrollId);
        if (entity == null || entity.TenantId != tenantId || !entity.IsActive) return false;

        if (string.Equals(entity.Status, "Posted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Payroll is already Posted.");

        if (string.Equals(entity.Status, "Processed", StringComparison.OrdinalIgnoreCase))
        {
            entity.Status = "Posted";
            entity.PostedBy = userId == Guid.Empty ? null : userId;
            entity.PostedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
            return await _payrolls.UpdateAsync(entity);
        }

        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Cannot process payroll in status '{entity.Status}'.");

        await GenerateLinesAsync(entity, tenantId, userId);
        return true;
    }

    private async Task GenerateLinesAsync(Payroll payroll, Guid tenantId, Guid userId)
    {
        var existing = (await _lines.GetByPayrollAsync(payroll.Id)).ToList();
        foreach (var line in existing)
            await _lines.DeleteAsync(line.Id);

        var employees = await LoadEmployeesAsync(payroll, tenantId);
        if (employees.Count == 0)
            throw new InvalidOperationException("No active employees found for this payroll run.");

        decimal totalGross = 0, totalDed = 0, totalNet = 0;
        foreach (var emp in employees)
        {
            var baseSalary = emp.EmploymentContract?.BaseSalary > 0
                ? emp.EmploymentContract.BaseSalary
                : payroll.BasicSalary;
            if (baseSalary <= 0) continue;

            var allowances = payroll.Allowances;
            var otherDed = payroll.Deductions;
            var gross = Math.Round(baseSalary + allowances, 2);
            var tax = Math.Round(gross * DefaultTaxRate, 2);
            var deductions = Math.Round(otherDed + tax, 2);
            var net = Math.Round(gross - deductions, 2);
            if (net < 0) net = 0;

            var line = new PayrollLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PayrollId = payroll.Id,
                EmployeeId = emp.Id,
                Earnings = baseSalary,
                Allowances = allowances,
                Deductions = otherDed,
                TaxAmount = tax,
                NetPay = net,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
                IsActive = true
            };
            await _lines.CreateAsync(line);

            totalGross += gross;
            totalDed += deductions;
            totalNet += net;
        }

        if (totalGross == 0 && totalNet == 0)
            throw new InvalidOperationException(
                "No employees with a base salary (contract or payroll default) to process.");

        payroll.TotalGross = Math.Round(totalGross, 2);
        payroll.TotalDeductions = Math.Round(totalDed, 2);
        payroll.TotalNet = Math.Round(totalNet, 2);
        payroll.GrossSalary = payroll.TotalGross;
        payroll.NetSalary = payroll.TotalNet;
        payroll.Deductions = payroll.TotalDeductions;
        payroll.Status = "Processed";
        payroll.ProcessedAt = DateTime.UtcNow;
        payroll.PayrollRunDate = DateTime.UtcNow;
        payroll.UpdatedAt = DateTime.UtcNow;
        payroll.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _payrolls.UpdateAsync(payroll);
    }

    private async Task<List<Employee>> LoadEmployeesAsync(Payroll payroll, Guid tenantId)
    {
        if (payroll.EmployeeId.HasValue && payroll.EmployeeId != Guid.Empty)
        {
            var one = await _employees.GetEmployeeByIdAsync(payroll.EmployeeId.Value);
            if (one == null || one.TenantId != tenantId || one.IsDeleted
                || !string.Equals(one.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Target employee is not an active employee for this tenant.");
            return [one];
        }

        var page = await _employees.GetAllEmployeesAsync(new EmployeeQuery
        {
            TenantId = tenantId,
            CompanyId = payroll.CompanyId == Guid.Empty ? null : payroll.CompanyId,
            Status = "Active",
            Page = 1,
            PageSize = 5000
        });
        return page.Items.ToList();
    }

    private async Task<Payroll> RequireDraftAsync(Guid id, Guid tenantId)
    {
        var entity = await _payrolls.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Payroll '{id}' not found.");
        if (entity.TenantId != tenantId || !entity.IsActive)
            throw new KeyNotFoundException($"Payroll '{id}' not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft payroll can be updated.");
        return entity;
    }

    private static PayrollDto Map(Payroll p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        CompanyId = p.CompanyId == Guid.Empty ? null : p.CompanyId,
        EmployeeId = p.EmployeeId,
        PayPeriodStart = p.PayPeriodStart,
        PayPeriodEnd = p.PayPeriodEnd,
        PayDate = p.PayDate,
        Status = p.Status,
        Currency = p.Currency,
        TotalGross = p.TotalGross,
        TotalDeductions = p.TotalDeductions,
        TotalNet = p.TotalNet,
        EmployeeCount = p.PayrollLines?.Count ?? 0,
        ProcessedAt = p.ProcessedAt,
        ProcessedBy = p.PostedBy,
        BasicSalary = p.BasicSalary,
        Allowances = p.Allowances,
        Deductions = p.Deductions,
        GrossSalary = p.GrossSalary,
        NetSalary = p.NetSalary,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        CreatedBy = p.CreatedBy,
        UpdatedBy = p.UpdatedBy,
        IsActive = p.IsActive,
        PayrollNumber = $"PR-{p.PayPeriodStart:yyyyMM}-{p.Id.ToString("N")[..8]}"
    };
}
