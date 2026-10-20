using ERP.Api.Errors;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Api.Validators.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/taxes")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TaxController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITaxService _service;
    private readonly CreateTaxRateValidator _createValidator;
    private readonly UpdateTaxRateValidator _updateValidator;

    public TaxController(
        ITaxService service,
        CreateTaxRateValidator createValidator,
        UpdateTaxRateValidator updateValidator,
        ILogger<TaxController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxRateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaxRateDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync());

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<TaxRateDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetActiveRates() =>
        await HandleAsync(() => _service.GetActiveRatesAsync());

    [HttpGet("type/{taxType}")]
    [ProducesResponseType(typeof(IEnumerable<TaxRateDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByType(string taxType) =>
        await HandleAsync(() => _service.GetByTypeAsync(taxType));

    [HttpPost]
    [ProducesResponseType(typeof(TaxRateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaxRateDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id));
}
