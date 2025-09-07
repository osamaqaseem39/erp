using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using FluentValidation;
using ERP.Api.Validators.CRM;

namespace ERP.Api.Controllers.CRM;

[ApiController]
[Route("api/crm/quotes")]
public class QuoteController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IQuoteService _service;
    private readonly CreateQuoteValidator _createValidator;
    private readonly UpdateQuoteValidator _updateValidator;

    public QuoteController(
        IQuoteService service,
        CreateQuoteValidator createValidator,
        UpdateQuoteValidator updateValidator,
        ILogger<QuoteController> logger) : base(logger)
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

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        return await HandleAsync(() => _service.GetByCustomerAsync(customerId));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuoteDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuoteDto dto)
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

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendQuote(Guid id)
    {
        return await HandleAsync(() => _service.SendQuoteAsync(id));
    }

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> ConvertToSalesOrder(Guid id)
    {
        return await HandleAsync(() => _service.ConvertToSalesOrderAsync(id));
    }
}