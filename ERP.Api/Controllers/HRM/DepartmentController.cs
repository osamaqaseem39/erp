using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/departments")]
[Authorize]
public class DepartmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger) : base(logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _departmentService.CreateAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var department = await _departmentService.GetByIdAsync(id, tenantId);
        if (department == null)
            return NotFound();
        return Ok(department);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId = null)
    {
        var tenantId = GetTenantId();
        var departments = await _departmentService.GetAllAsync(tenantId, companyId);
        return Ok(departments);
    }

    [HttpPut("{id}")]
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