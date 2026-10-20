using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/budgets")]
[Authorize]
[PermissionResource("finance.budgets")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BudgetsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBudgetService _service;

    public BudgetsController(IBudgetService service, ILogger<BudgetsController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List() =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BudgetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto)
    {
        var row = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = row.Id }, row);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BudgetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetDto dto)
    {
        dto.Id = id;
        var row = await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Ok(row);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Activate(Guid id) =>
        await _service.ActivateAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new { success = true })
            : BadRequest(new { error = "Only draft budgets can be activated." });

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Close(Guid id) =>
        await _service.CloseAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new { success = true })
            : BadRequest(new { error = "Only active budgets can be closed." });
}
