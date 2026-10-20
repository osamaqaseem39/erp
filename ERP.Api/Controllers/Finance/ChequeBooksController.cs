using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/cheque-books")]
[Authorize]
[PermissionResource("finance.chequeBooks")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ChequeBooksController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IChequeBookService _service;

    public ChequeBooksController(IChequeBookService service, ILogger<ChequeBooksController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChequeBookDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChequeBookDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Get(Guid id)
    {
        var row = await _service.GetAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChequeBookDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateChequeBookDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/leaves/{leafNumber:int}/issue")]
    [ProducesResponseType(typeof(ChequeLeafDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> IssueLeaf(Guid id, int leafNumber, [FromBody] IssueChequeLeafDto dto) =>
        Ok(await _service.IssueLeafAsync(id, leafNumber, dto, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/leaves/{leafNumber:int}/status")]
    [ProducesResponseType(typeof(ChequeLeafDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> SetLeafStatus(Guid id, int leafNumber, [FromBody] SetChequeLeafStatusDto dto) =>
        Ok(await _service.SetLeafStatusAsync(id, leafNumber, dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
