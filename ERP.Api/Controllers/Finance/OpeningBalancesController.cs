using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/opening-balances")]
[Authorize]
[PermissionResource("finance.openingBalances")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class OpeningBalancesController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IOpeningBalanceService _service;

    public OpeningBalancesController(IOpeningBalanceService service, ILogger<OpeningBalancesController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OpeningBalanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? fiscalYearId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), fiscalYearId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OpeningBalanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OpeningBalanceDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateOpeningBalanceDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(OpeningBalanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Post(Guid id) =>
        Ok(await _service.PostAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));
}
