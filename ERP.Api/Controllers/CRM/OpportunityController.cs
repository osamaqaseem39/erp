using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using FluentValidation;
using ERP.Api.Validators.CRM;

namespace ERP.Api.Controllers.CRM;

[ApiController]
[Route("api/crm/opportunities")]
public class OpportunityController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IOpportunityService _service;
    private readonly CreateOpportunityValidator _createValidator;
    private readonly UpdateOpportunityValidator _updateValidator;

    public OpportunityController(
        IOpportunityService service,
        CreateOpportunityValidator createValidator,
        UpdateOpportunityValidator updateValidator,
        ILogger<OpportunityController> logger) : base(logger)
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

    [HttpGet("assigned/{assignedTo}")]
    public async Task<IActionResult> GetByAssignedTo(Guid assignedTo)
    {
        return await HandleAsync(() => _service.GetByAssignedToAsync(assignedTo));
    }

    [HttpGet("stage/{stage}")]
    public async Task<IActionResult> GetByStage(string stage)
    {
        return await HandleAsync(() => _service.GetByStageAsync(stage));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
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

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> ConvertToQuote(Guid id)
    {
        return await HandleAsync(() => _service.ConvertToQuoteAsync(id));
    }
}