using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Manufacturing;

[ApiController]
[Route("api/manufacturing/quality-checks")]
[Authorize]
[PermissionResource("manufacturing.qualityChecks")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class QualityCheckController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IQualityCheckService _service;

    public QualityCheckController(IQualityCheckService service, ILogger<QualityCheckController> logger) : base(logger) =>
        _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QualityCheckDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid? productionOrderId = null) =>
        Ok(await _service.ListAsync(HttpContext.RequireTenantId(), productionOrderId));

    [HttpPost]
    [ProducesResponseType(typeof(QualityCheckDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateQualityCheckDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(List), new { productionOrderId = result.ProductionOrderId }, result);
    }

    [HttpPost("{id:guid}/pass")]
    [ProducesResponseType(typeof(QualityCheckDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Pass(Guid id, [FromBody] PassFailQualityCheckDto? dto) =>
        Ok(await _service.PassAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpPost("{id:guid}/fail")]
    [ProducesResponseType(typeof(QualityCheckDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Fail(Guid id, [FromBody] PassFailQualityCheckDto? dto) =>
        Ok(await _service.FailAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId()));
}
