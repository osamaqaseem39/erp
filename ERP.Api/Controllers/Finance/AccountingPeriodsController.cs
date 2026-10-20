using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/accounting-periods")]
[Authorize]
[PermissionResource("finance.accountingPeriods")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class AccountingPeriodsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IAccountingPeriodService _service;

    public AccountingPeriodsController(IAccountingPeriodService service, ILogger<AccountingPeriodsController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountingPeriodDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? fiscalYearId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), fiscalYearId));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountingPeriodDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountingPeriodDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateAccountingPeriodDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(AccountingPeriodDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Close(Guid id) =>
        Ok(await _service.CloseAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/open")]
    [ProducesResponseType(typeof(AccountingPeriodDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Open(Guid id) =>
        Ok(await _service.OpenAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));
}
