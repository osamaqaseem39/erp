using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/departments")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class DepartmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger) : base(logger)
    {
        _departmentService = departmentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _departmentService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DepartmentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var department = await _departmentService.GetByIdAsync(id, tenantId);
        if (department == null)
            return NotFound();
        return Ok(department);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId = null)
    {
        var tenantId = GetTenantId();
        var departments = await _departmentService.GetAllAsync(tenantId, companyId);
        return Ok(departments);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DepartmentDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _departmentService.UpdateAsync(dto, tenantId, userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _departmentService.DeleteAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
