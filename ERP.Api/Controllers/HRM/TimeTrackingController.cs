using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Shared.DTOs.HRM;
using ERP.Application.Interfaces.HRM;
using ERP.Api.Validators.HRM;
using FluentValidation;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/time-tracking")]
public class TimeTrackingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITimeTrackingService _service;
    private readonly CreateTimeEntryValidator _createValidator;
    private readonly UpdateTimeEntryValidator _updateValidator;

    public TimeTrackingController(
        ITimeTrackingService service,
        CreateTimeEntryValidator createValidator,
        UpdateTimeEntryValidator updateValidator,
        ILogger<TimeTrackingController> logger) : base(logger)
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

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        return await HandleAsync(() => _service.GetByEmployeeAsync(employeeId));
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));
    }

    [HttpGet("employee/{employeeId}/total-hours")]
    public async Task<IActionResult> GetTotalHours(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetTotalHoursAsync(employeeId, startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryDto dto)
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