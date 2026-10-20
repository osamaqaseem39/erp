using ERP.Api.Security;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/fixed-assets")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class FixedAssetController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IFixedAssetService _service;
    private readonly CreateFixedAssetValidator _createValidator;
    private readonly UpdateFixedAssetValidator _updateValidator;

    public FixedAssetController(
        IFixedAssetService service,
        CreateFixedAssetValidator createValidator,
        UpdateFixedAssetValidator updateValidator,
        ILogger<FixedAssetController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FixedAssetDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAssetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Search([FromQuery] FixedAssetQuery query)
    {
        query.TenantId = GetTenantId();
        var (assets, totalCount) = await _service.SearchAssetsAsync(query);
        return Ok(new { Assets = assets, TotalCount = totalCount, query.PageNumber, query.PageSize });
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var id = await _service.AddAssetAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFixedAssetDto dto)
    {
        dto.AssetId = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UpdateAssetAsync(dto, tenantId, userId);
        return result != Guid.Empty ? Ok(new { id = result }) : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.DeleteAssetAsync(id, tenantId);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/dispose")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> DisposeAsset(Guid id, [FromBody] DisposeAssetDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.DisposeAssetAsync(id, dto.DisposalValue, dto.DisposalDate, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("{id}/undo-disposal")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UndoDisposal(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UndoDisposalAsync(id, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("depreciation/run")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> RunDepreciation([FromBody] DepreciationRunDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.RunDepreciationAsync(dto, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpGet("{id}/depreciation")]
    [ProducesResponseType(typeof(IEnumerable<DepreciationEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDepreciationEntries(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetDepreciationEntriesAsync(id, tenantId);
        return Ok(result);
    }

    [HttpGet("depreciation/period")]
    [ProducesResponseType(typeof(IEnumerable<DepreciationEntryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDepreciationEntriesByPeriod([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetDepreciationEntriesByPeriodAsync(tenantId, periodStart, periodEnd);
        return Ok(result);
    }

    [HttpPost("depreciation/post")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostDepreciationForPeriod([FromBody] PostDepreciationDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.PostDepreciationForPeriodAsync(dto.PeriodStart, dto.PeriodEnd, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpGet("depreciation/methods")]
    [ProducesResponseType(typeof(IEnumerable<string>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAvailableDepreciationMethods()
    {
        var result = await _service.GetAvailableDepreciationMethodsAsync();
        return Ok(result);
    }

    [HttpPost("{id}/calculate-depreciation")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CalculateDepreciation(Guid id, [FromBody] CalculateDepreciationDto dto)
    {
        var tenantId = GetTenantId();
        var result = await _service.CalculateDepreciationAsync(id, dto.PeriodStart, dto.PeriodEnd, tenantId);
        return Ok(new { Amount = result });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}

// DTOs for additional operations
public class DisposeAssetDto
{
    public decimal DisposalValue { get; set; }
    public DateTime DisposalDate { get; set; }
}

public class PostDepreciationDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class CalculateDepreciationDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}