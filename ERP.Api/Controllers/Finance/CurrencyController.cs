using ERP.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces;
using ERP.Api.Validators.Finance;
using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/currencies")]
[Authorize]
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
    public async Task<IActionResult> GetById(string currencyCode)
    {
        return await HandleAsync(() => _service.GetByIdAsync(currencyCode));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        return await HandleAsync(() => _service.GetActiveAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{currencyCode}")]
    public async Task<IActionResult> Update(string currencyCode, [FromBody] UpdateCurrencyDto dto)
    {
        dto.CurrencyCode = currencyCode;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{currencyCode}")]
    public async Task<IActionResult> Delete(string currencyCode)
    {
        return await HandleAsync(() => _service.DeleteAsync(currencyCode));
    }

    [HttpGet("{currencyCode}/exists")]
    public async Task<IActionResult> Exists(string currencyCode)
    {
        return await HandleAsync(() => _service.ExistsAsync(currencyCode));
    }
}