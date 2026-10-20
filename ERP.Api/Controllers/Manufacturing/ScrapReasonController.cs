using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/scrap-reasons")]
[Authorize]
[PermissionResource("manufacturing.scrapReasons")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ScrapReasonController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IScrapReasonService _service;

    public ScrapReasonController(IScrapReasonService service, ILogger<ScrapReasonController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScrapReasonDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync(HttpContext.RequireTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(ScrapReasonDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateScrapReasonDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(List), result);
    }
}
