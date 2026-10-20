using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/expense-claims")]
[Authorize]
[PermissionResource("finance.expenseClaims")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ExpenseClaimsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IExpenseClaimService _service;

    public ExpenseClaimsController(IExpenseClaimService service, ILogger<ExpenseClaimsController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExpenseClaimDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] string? status = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), status));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseClaimDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExpenseClaimDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseClaimDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ExpenseClaimDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Submit(Guid id) =>
        Ok(await _service.SubmitAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ExpenseClaimDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id) =>
        Ok(await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(ExpenseClaimDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkPaid(Guid id) =>
        Ok(await _service.MarkPaidAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));
}
