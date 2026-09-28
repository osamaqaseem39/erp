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
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAssetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] FixedAssetQuery query)
    {
        var tenantId = GetTenantId();
        var result = await _service.SearchAssetsAsync(query);
        return Ok(result);
    }

    [HttpPost]
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
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.DeleteAssetAsync(id, tenantId);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/dispose")]
    public async Task<IActionResult> DisposeAsset(Guid id, [FromBody] DisposeAssetDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.DisposeAssetAsync(id, dto.DisposalValue, dto.DisposalDate, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("{id}/undo-disposal")]
    public async Task<IActionResult> UndoDisposal(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.UndoDisposalAsync(id, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("depreciation/run")]
    public async Task<IActionResult> RunDepreciation([FromBody] DepreciationRunDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.RunDepreciationAsync(dto, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpGet("{id}/depreciation")]
    public async Task<IActionResult> GetDepreciationEntries(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetDepreciationEntriesAsync(id, tenantId);
        return Ok(result);
    }

    [HttpGet("depreciation/period")]
    public async Task<IActionResult> GetDepreciationEntriesByPeriod([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetDepreciationEntriesByPeriodAsync(tenantId, periodStart, periodEnd);
        return Ok(result);
    }

    [HttpPost("depreciation/post")]
    public async Task<IActionResult> PostDepreciationForPeriod([FromBody] PostDepreciationDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _service.PostDepreciationForPeriodAsync(dto.PeriodStart, dto.PeriodEnd, tenantId, userId);
        return result ? Ok() : BadRequest();
    }

    [HttpGet("depreciation/methods")]
    public async Task<IActionResult> GetAvailableDepreciationMethods()
    {
        var result = await _service.GetAvailableDepreciationMethodsAsync();
        return Ok(result);
    }

    [HttpPost("{id}/calculate-depreciation")]
    public async Task<IActionResult> CalculateDepreciation(Guid id, [FromBody] CalculateDepreciationDto dto)
    {
        var tenantId = GetTenantId();
        var result = await _service.CalculateDepreciationAsync(id, dto.PeriodStart, dto.PeriodEnd, tenantId);
        return Ok(new { Amount = result });
    }

    private Guid GetTenantId()
    {
        // Implementation to get tenant ID from context
        return Guid.NewGuid(); // Placeholder
    }

    private Guid GetUserId()
    {
        // Implementation to get user ID from context
        return Guid.NewGuid(); // Placeholder
    }
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