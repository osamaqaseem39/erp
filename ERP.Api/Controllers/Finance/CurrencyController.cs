using ERP.Api.Errors;
using ERP.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces;
using ERP.Api.Validators.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/currencies")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CurrencyController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICurrencyService _service;
    private readonly CreateCurrencyValidator _createValidator;
    private readonly UpdateCurrencyValidator _updateValidator;

    public CurrencyController(
        ICurrencyService service,
        CreateCurrencyValidator createValidator,
        UpdateCurrencyValidator updateValidator,
        ILogger<CurrencyController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{currencyCode}")]
    [ProducesResponseType(typeof(CurrencyDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(string currencyCode) =>
        await HandleAsync(() => _service.GetByIdAsync(currencyCode));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync());

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetActive() =>
        await HandleAsync(() => _service.GetActiveAsync());

    [HttpPost]
    [ProducesResponseType(typeof(CurrencyDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{currencyCode}")]
    [ProducesResponseType(typeof(CurrencyDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(string currencyCode, [FromBody] UpdateCurrencyDto dto)
    {
        dto.CurrencyCode = currencyCode;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{currencyCode}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(string currencyCode) =>
        await HandleAsync(() => _service.DeleteAsync(currencyCode));

    [HttpGet("{currencyCode}/exists")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Exists(string currencyCode) =>
        await HandleAsync(() => _service.ExistsAsync(currencyCode));
}
