using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;
using FluentValidation;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/leads")]
public class LeadController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ILeadService _service;
    private readonly CreateLeadValidator _createValidator;
    private readonly UpdateLeadValidator _updateValidator;

    public LeadController(
        ILeadService service,
        CreateLeadValidator createValidator,
        UpdateLeadValidator updateValidator,
        ILogger<LeadController> logger) : base(logger)
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

    [HttpGet("source/{source}")]
    public async Task<IActionResult> GetBySource(string source)
    {
        return await HandleAsync(() => _service.GetBySourceAsync(source));
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadDto dto)
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
    public async Task<IActionResult> ConvertToCustomer(Guid id)
    {
        return await HandleAsync(() => _service.ConvertToCustomerAsync(id));
    }

    [HttpPost("{id}/convert-to-opportunity")]
    public async Task<IActionResult> ConvertToOpportunity(Guid id)
    {
        return await HandleAsync(() => _service.ConvertToOpportunityAsync(id));
    }
}