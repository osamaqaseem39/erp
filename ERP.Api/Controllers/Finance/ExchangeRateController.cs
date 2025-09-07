using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/exchange-rates")]
public class ExchangeRateController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IExchangeRateService _service;
    private readonly CreateExchangeRateValidator _createValidator;
    private readonly UpdateExchangeRateValidator _updateValidator;

    public ExchangeRateController(
        IExchangeRateService service,
        CreateExchangeRateValidator createValidator,
        UpdateExchangeRateValidator updateValidator,
        ILogger<ExchangeRateController> logger) : base(logger)
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

    [HttpGet("currency/{fromCurrency}/{toCurrency}")]
    public async Task<IActionResult> GetByCurrency(string fromCurrency, string toCurrency)
    {
        return await HandleAsync(() => _service.GetByCurrencyAsync(fromCurrency, toCurrency));
    }

    [HttpGet("current/{fromCurrency}/{toCurrency}")]
    public async Task<IActionResult> GetCurrentRate(string fromCurrency, string toCurrency)
    {
        return await HandleAsync(() => _service.GetCurrentRateAsync(fromCurrency, toCurrency));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExchangeRateDto dto)
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

    [HttpPost("convert")]
    public async Task<IActionResult> ConvertAmount([FromBody] ConvertAmountRequest request)
    {
        return await HandleAsync(() => _service.ConvertAmountAsync(
            request.Amount, 
            request.FromCurrency, 
            request.ToCurrency, 
            request.AsOfDate));
    }
}

public class ConvertAmountRequest
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public DateTime? AsOfDate { get; set; }
}