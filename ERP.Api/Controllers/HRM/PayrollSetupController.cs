using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/payroll-components")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PayrollComponentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly PayrollSetupService _service;

    public PayrollComponentController(PayrollSetupService service, ILogger<PayrollComponentController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PayrollComponentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetComponentsAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PayrollComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetComponentAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PayrollComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] PayrollComponentDto dto)
    {
        dto.Id = Guid.Empty;
        return await HandleAsync(() => _service.SaveComponentAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId()));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PayrollComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PayrollComponentDto dto)
    {
        dto.Id = id;
        return await HandleAsync(() => _service.SaveComponentAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId()));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteComponentAsync(id, HttpContext.RequireTenantId()));
}

[ApiController]
[Route("api/hrm/salary-structures")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalaryStructureController : ERP.Api.Abstractions.ControllerBase
{
    private readonly PayrollSetupService _service;

    public SalaryStructureController(PayrollSetupService service, ILogger<SalaryStructureController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeSalaryComponentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? employeeId) =>
        Ok(await _service.GetStructuresAsync(HttpContext.RequireTenantId(), employeeId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeSalaryComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var row = await _service.GetStructureAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeSalaryComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] EmployeeSalaryComponentDto dto)
    {
        dto.Id = Guid.Empty;
        return await HandleAsync(() => _service.SaveStructureAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId()));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeSalaryComponentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeSalaryComponentDto dto)
    {
        dto.Id = id;
        return await HandleAsync(() => _service.SaveStructureAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId()));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteStructureAsync(id, HttpContext.RequireTenantId()));
}
