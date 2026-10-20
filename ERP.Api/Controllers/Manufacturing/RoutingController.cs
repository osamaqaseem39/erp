using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/routings")]
[Authorize]
[PermissionResource("manufacturing.routings")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class RoutingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IRoutingService _service;

    public RoutingController(IRoutingService service, ILogger<RoutingController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoutingDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoutingDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoutingDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateRoutingDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(RoutingDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id) =>
        Ok(await _service.ActivateAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/obsolete")]
    [ProducesResponseType(typeof(RoutingDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Obsolete(Guid id) =>
        Ok(await _service.ObsoleteAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));
}
