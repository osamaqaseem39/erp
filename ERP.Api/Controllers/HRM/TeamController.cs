using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/teams")]
[Authorize]
[PermissionResource("hrm.teams")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TeamController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITeamService _service;

    public TeamController(ITeamService service, ILogger<TeamController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] bool? isActive = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), isActive));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateTeamDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamDto dto) =>
        Ok(await _service.UpdateAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
