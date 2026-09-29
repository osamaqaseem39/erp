using ERP.Api.Security;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Admin;

/// <summary>
/// Read-only view of the current tenant's system log (server errors and security events).
/// No create/update/delete endpoints: a log that callers can edit can't be trusted.
/// </summary>
[ApiController]
[Route("api/admin/system-logs")]
[Authorize]
public class SystemLogController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISystemLogService _service;

    public SystemLogController(ISystemLogService service, ILogger<SystemLogController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] SystemLogPagedQuery query, CancellationToken cancellationToken)
    {
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

    [HttpGet("level/{logLevel}")]
    public async Task<IActionResult> GetByLevel(string logLevel) => Ok(await _service.GetByLevelAsync(logLevel));

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId) => Ok(await _service.GetByUserAsync(userId));

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate) =>
        Ok(await _service.GetByDateRangeAsync(startDate, endDate));
}
