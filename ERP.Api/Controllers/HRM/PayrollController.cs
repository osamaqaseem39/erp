using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/payroll")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PayrollController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(
        IPayrollService service,
        ILogger<PayrollController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PayrollDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetByIdAsync(id, tenantId));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PayrollDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetAllAsync(tenantId));
    }

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<PayrollDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetByEmployeeAsync(employeeId, tenantId));
    }

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<PayrollDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByPeriod(DateTime startDate, DateTime endDate)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.GetByPeriodAsync(startDate, endDate, tenantId));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] PayrollDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        return await HandleAsync(() => _service.CreateAsync(dto, tenantId, userId));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PayrollDto dto)
    {
        dto.Id = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        return await HandleAsync(() => _service.UpdateAsync(dto, tenantId, userId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        return await HandleAsync(() => _service.DeleteAsync(id, tenantId));
    }

    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Process(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        return await HandleAsync(() => _service.ProcessPayrollAsync(id, tenantId, userId));
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
