using System.Text.Json;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.HRM;

/// <summary>Salary components, per-employee salary structures and payslips.</summary>
public class PayrollSetupService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly ERPDbContext _db;

    public PayrollSetupService(ERPDbContext db) => _db = db;

    // ── Components ────────────────────────────────────────────────

    public async Task<IEnumerable<PayrollComponentDto>> GetComponentsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PayrollComponentDto>();
        return (await _db.PayrollComponents.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .ToListAsync()).Select(Map);
    }

    public async Task<PayrollComponentDto?> GetComponentAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var c = await _db.PayrollComponents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        return c == null ? null : Map(c);
    }

    public async Task<PayrollComponentDto> SaveComponentAsync(PayrollComponentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.");
        var type = Normalize(dto.Type, SalaryCalculator.Types, "Type");
        var calc = Normalize(dto.CalculationType, SalaryCalculator.CalculationTypes, "Calculation");
        if (type == "Basic" && calc != "Fixed") throw new ArgumentException("Basic salary must be a fixed amount.");
        if (type == "Earning" && calc == "PercentOfGross") throw new ArgumentException("Earnings can be fixed or a percent of basic.");
        if (dto.Value < 0) throw new ArgumentException("Value cannot be negative.");

        PayrollComponent entity;
        if (dto.Id == Guid.Empty)
        {
            entity = new PayrollComponent { Id = Guid.NewGuid(), TenantId = tenantId, CreatedAt = DateTime.UtcNow, CreatedBy = User(userId) };
            _db.PayrollComponents.Add(entity);
        }
        else
        {
            entity = await _db.PayrollComponents.FirstOrDefaultAsync(c => c.Id == dto.Id && c.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Payroll component '{dto.Id}' not found.");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User(userId);
        }

        entity.Name = dto.Name.Trim();
        entity.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpperInvariant();
        entity.Type = type;
        entity.CalculationType = calc;
        entity.Value = dto.Value;
        entity.IsTaxable = dto.IsTaxable;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<bool> DeleteComponentAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.PayrollComponents.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (entity == null) return false;
        if (await _db.EmployeeSalaryComponents.AnyAsync(s => s.PayrollComponentId == id))
            throw new InvalidOperationException("Component is used in salary structures. Deactivate it instead.");
        _db.PayrollComponents.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Salary structures ─────────────────────────────────────────

    public async Task<IEnumerable<EmployeeSalaryComponentDto>> GetStructuresAsync(Guid tenantId, Guid? employeeId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<EmployeeSalaryComponentDto>();
        var q = _db.EmployeeSalaryComponents.AsNoTracking()
            .Include(s => s.Employee).Include(s => s.PayrollComponent)
            .Where(s => s.TenantId == tenantId);
        if (employeeId.HasValue) q = q.Where(s => s.EmployeeId == employeeId);
        return (await q.ToListAsync())
            .OrderBy(s => s.Employee.FirstName).ThenBy(s => s.Employee.LastName)
            .ThenBy(s => s.PayrollComponent.DisplayOrder).ThenBy(s => s.PayrollComponent.Name)
            .Select(Map);
    }

    public async Task<EmployeeSalaryComponentDto?> GetStructureAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var s = await _db.EmployeeSalaryComponents.AsNoTracking()
            .Include(x => x.Employee).Include(x => x.PayrollComponent)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        return s == null ? null : Map(s);
    }

    public async Task<EmployeeSalaryComponentDto> SaveStructureAsync(EmployeeSalaryComponentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        if (!await _db.Employees.AnyAsync(e => e.Id == dto.EmployeeId && e.TenantId == tenantId))
            throw new ArgumentException("Employee not found.");
        if (!await _db.PayrollComponents.AnyAsync(c => c.Id == dto.PayrollComponentId && c.TenantId == tenantId))
            throw new ArgumentException("Payroll component not found.");
        if (dto.Value < 0) throw new ArgumentException("Value cannot be negative.");
        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveTo < dto.EffectiveFrom)
            throw new ArgumentException("Effective to must be on or after effective from.");
        if (dto.IsActive && await _db.EmployeeSalaryComponents.AnyAsync(s => s.TenantId == tenantId && s.Id != dto.Id
                && s.EmployeeId == dto.EmployeeId && s.PayrollComponentId == dto.PayrollComponentId && s.IsActive))
            throw new InvalidOperationException("This employee already has this component. Edit the existing row.");

        EmployeeSalaryComponent entity;
        if (dto.Id == Guid.Empty)
        {
            entity = new EmployeeSalaryComponent { Id = Guid.NewGuid(), TenantId = tenantId, CreatedAt = DateTime.UtcNow, CreatedBy = User(userId) };
            _db.EmployeeSalaryComponents.Add(entity);
        }
        else
        {
            entity = await _db.EmployeeSalaryComponents.FirstOrDefaultAsync(s => s.Id == dto.Id && s.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Salary structure row '{dto.Id}' not found.");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User(userId);
        }

        entity.EmployeeId = dto.EmployeeId;
        entity.PayrollComponentId = dto.PayrollComponentId;
        entity.Value = dto.Value;
        entity.EffectiveFrom = dto.EffectiveFrom?.Date;
        entity.EffectiveTo = dto.EffectiveTo?.Date;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return (await GetStructureAsync(entity.Id, tenantId))!;
    }

    public async Task<bool> DeleteStructureAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.EmployeeSalaryComponents.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        if (entity == null) return false;
        _db.EmployeeSalaryComponents.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Payslips ──────────────────────────────────────────────────

    public async Task<IEnumerable<PayslipDto>> GetPayslipsAsync(Guid payrollId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PayslipDto>();
        var payroll = await _db.Payrolls.AsNoTracking().FirstOrDefaultAsync(p => p.Id == payrollId && p.TenantId == tenantId && p.IsActive);
        if (payroll == null) return Array.Empty<PayslipDto>();

        var lines = await _db.PayrollLines.AsNoTracking().Include(l => l.Employee)
            .Where(l => l.PayrollId == payrollId && l.TenantId == tenantId)
            .ToListAsync();

        return lines.OrderBy(l => l.Employee.FirstName).ThenBy(l => l.Employee.LastName).Select(l =>
        {
            var items = string.IsNullOrWhiteSpace(l.BreakdownJson)
                ? Legacy(l)
                : JsonSerializer.Deserialize<List<PayslipItemDto>>(l.BreakdownJson, Json) ?? Legacy(l);
            return new PayslipDto
            {
                LineId = l.Id,
                PayrollId = payroll.Id,
                PayrollNumber = PayrollCrudService.NumberOf(payroll),
                Status = payroll.Status,
                Currency = payroll.Currency,
                PayPeriodStart = payroll.PayPeriodStart,
                PayPeriodEnd = payroll.PayPeriodEnd,
                PayDate = payroll.PayDate,
                EmployeeId = l.EmployeeId,
                EmployeeName = $"{l.Employee.FirstName} {l.Employee.LastName}".Trim(),
                EmployeeCode = l.Employee.EmployeeCode,
                JobTitle = l.Employee.JobTitle,
                BasicSalary = l.Earnings,
                Allowances = l.Allowances,
                GrossPay = l.Earnings + l.Allowances,
                Deductions = l.Deductions,
                TaxAmount = l.TaxAmount,
                NetPay = l.NetPay,
                Earnings = items.Where(i => i.Type is "Basic" or "Earning").ToList(),
                DeductionItems = items.Where(i => i.Type is "Deduction" or "Tax").ToList(),
            };
        }).ToList();
    }

    public static string Serialize(IEnumerable<PayslipItemDto> items) => JsonSerializer.Serialize(items, Json);

    private static List<PayslipItemDto> Legacy(PayrollLine l)
    {
        var list = new List<PayslipItemDto> { new() { Name = "Basic salary", Type = "Basic", Amount = l.Earnings } };
        if (l.Allowances != 0) list.Add(new() { Name = "Allowances", Type = "Earning", Amount = l.Allowances });
        if (l.Deductions != 0) list.Add(new() { Name = "Deductions", Type = "Deduction", Amount = l.Deductions });
        if (l.TaxAmount != 0) list.Add(new() { Name = "Income tax", Type = "Tax", Amount = l.TaxAmount });
        return list;
    }

    private static string Normalize(string? value, string[] allowed, string label) =>
        allowed.FirstOrDefault(a => string.Equals(a, value?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"{label} must be one of: {string.Join(", ", allowed)}.");

    private static string? User(Guid userId) => userId == Guid.Empty ? null : userId.ToString();

    private static PayrollComponentDto Map(PayrollComponent c) => new()
    {
        Id = c.Id, Name = c.Name, Code = c.Code, Type = c.Type, CalculationType = c.CalculationType,
        Value = c.Value, IsTaxable = c.IsTaxable, DisplayOrder = c.DisplayOrder, Description = c.Description, IsActive = c.IsActive,
    };

    private static EmployeeSalaryComponentDto Map(EmployeeSalaryComponent s) => new()
    {
        Id = s.Id,
        EmployeeId = s.EmployeeId,
        EmployeeName = s.Employee == null ? null : $"{s.Employee.FirstName} {s.Employee.LastName}".Trim(),
        EmployeeCode = s.Employee?.EmployeeCode,
        PayrollComponentId = s.PayrollComponentId,
        ComponentName = s.PayrollComponent?.Name,
        ComponentType = s.PayrollComponent?.Type,
        CalculationType = s.PayrollComponent?.CalculationType,
        Value = s.Value,
        EffectiveValue = s.Value ?? s.PayrollComponent?.Value ?? 0,
        EffectiveFrom = s.EffectiveFrom,
        EffectiveTo = s.EffectiveTo,
        IsActive = s.IsActive,
    };
}
