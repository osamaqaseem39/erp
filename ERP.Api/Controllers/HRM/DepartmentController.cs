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
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _departmentService.CreateAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var department = await _departmentService.GetByIdAsync(id, tenantId);
            if (department == null)
                return NotFound();
            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var departments = await _departmentService.GetAllAsync(tenantId, companyId);
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all departments");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _departmentService.UpdateAsync(dto, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _departmentService.DeleteAsync(id, tenantId);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;
        throw new UnauthorizedAccessException("Tenant ID not found in token");
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}