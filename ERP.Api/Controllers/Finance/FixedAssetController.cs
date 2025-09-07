using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/fixed-assets")]
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
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetAssetByIdAsync(id, tenantId);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] FixedAssetQuery query)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.SearchAssetsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetDto dto)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var tenantId = GetTenantId();
            var userId = GetUserId();
            var id = await _service.AddAssetAsync(dto, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFixedAssetDto dto)
    {
        try
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
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.DeleteAssetAsync(id, tenantId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/dispose")]
    public async Task<IActionResult> DisposeAsset(Guid id, [FromBody] DisposeAssetDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.DisposeAssetAsync(id, dto.DisposalValue, dto.DisposalDate, tenantId, userId);
            return result ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/undo-disposal")]
    public async Task<IActionResult> UndoDisposal(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.UndoDisposalAsync(id, tenantId, userId);
            return result ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("depreciation/run")]
    public async Task<IActionResult> RunDepreciation([FromBody] DepreciationRunDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.RunDepreciationAsync(dto, tenantId, userId);
            return result ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}/depreciation")]
    public async Task<IActionResult> GetDepreciationEntries(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetDepreciationEntriesAsync(id, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("depreciation/period")]
    public async Task<IActionResult> GetDepreciationEntriesByPeriod([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.GetDepreciationEntriesByPeriodAsync(tenantId, periodStart, periodEnd);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("depreciation/post")]
    public async Task<IActionResult> PostDepreciationForPeriod([FromBody] PostDepreciationDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var result = await _service.PostDepreciationForPeriodAsync(dto.PeriodStart, dto.PeriodEnd, tenantId, userId);
            return result ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("depreciation/methods")]
    public async Task<IActionResult> GetAvailableDepreciationMethods()
    {
        try
        {
            var result = await _service.GetAvailableDepreciationMethodsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/calculate-depreciation")]
    public async Task<IActionResult> CalculateDepreciation(Guid id, [FromBody] CalculateDepreciationDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _service.CalculateDepreciationAsync(id, dto.PeriodStart, dto.PeriodEnd, tenantId);
            return Ok(new { Amount = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
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