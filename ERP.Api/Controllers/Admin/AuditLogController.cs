using ERP.Api.Security;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

/// <summary>
/// Read-only view of the current tenant's audit trail. There are deliberately no write endpoints:
/// entries are produced by the server when data changes, so they can't be forged or edited.
/// </summary>
[ApiController]
[Route("api/admin/audit-logs")]
[Authorize]
public class AuditLogController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service, ILogger<AuditLogController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] AuditLogPagedQuery query, CancellationToken cancellationToken)
    {
        // Only SuperAdmin may look across tenants; for everyone else the flag is ignored.
        var allTenants = query.AllTenants && User.IsSuperAdmin();
        return Ok(await _service.GetPagedAsync(query, allTenants, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityName, string entityId) => Ok(await _service.GetByEntityAsync(entityName, entityId));

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId) => Ok(await _service.GetByUserAsync(userId));

    [HttpGet("action/{action}")]
    public async Task<IActionResult> GetByAction(string action) => Ok(await _service.GetByActionAsync(action));

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate) =>
        Ok(await _service.GetByDateRangeAsync(startDate, endDate));
}
