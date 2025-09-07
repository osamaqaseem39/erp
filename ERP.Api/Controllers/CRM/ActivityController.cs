using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;
using FluentValidation;

namespace ERP.Api.Controllers.CRM;

[ApiController]
[Route("api/crm/activities")]
public class ActivityController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IActivityService _service;
    private readonly CreateActivityValidator _createValidator;
    private readonly UpdateActivityValidator _updateValidator;

    public ActivityController(
        IActivityService service,
        CreateActivityValidator createValidator,
        UpdateActivityValidator updateValidator,
        ILogger<ActivityController> logger) : base(logger)
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

    [HttpGet("lead/{leadId}")]
    public async Task<IActionResult> GetByLead(Guid leadId)
    {
        return await HandleAsync(() => _service.GetByLeadAsync(leadId));
    }

    [HttpGet("opportunity/{opportunityId}")]
    public async Task<IActionResult> GetByOpportunity(Guid opportunityId)
    {
        return await HandleAsync(() => _service.GetByOpportunityAsync(opportunityId));
    }

    [HttpGet("assigned/{assignedTo}")]
    public async Task<IActionResult> GetByAssignedTo(Guid assignedTo)
    {
        return await HandleAsync(() => _service.GetByAssignedToAsync(assignedTo));
    }

    [HttpGet("type/{activityType}")]
    public async Task<IActionResult> GetByType(string activityType)
    {
        return await HandleAsync(() => _service.GetByTypeAsync(activityType));
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityDto dto)
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