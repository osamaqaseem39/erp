using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Api.Validators.Finance;
using ERP.Shared.DTOs.Finance;
using FluentValidation;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/taxes")]
[Authorize]
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
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRates()
    {
        return await HandleAsync(() => _service.GetActiveRatesAsync());
    }

    [HttpGet("type/{taxType}")]
    public async Task<IActionResult> GetByType(string taxType)
    {
        return await HandleAsync(() => _service.GetByTypeAsync(taxType));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id));
    }
}