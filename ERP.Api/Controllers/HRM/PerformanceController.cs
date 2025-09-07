using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Shared.DTOs.HRM;
using FluentValidation;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/performance")]
public class PerformanceController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPerformanceService _service;
    private readonly CreatePerformanceReviewValidator _createValidator;
    private readonly UpdatePerformanceReviewValidator _updateValidator;

    public PerformanceController(
        IPerformanceService service,
        CreatePerformanceReviewValidator createValidator,
        UpdatePerformanceReviewValidator updateValidator,
        ILogger<PerformanceController> logger) : base(logger)
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

    [HttpGet("reviewer/{reviewerId}")]
    public async Task<IActionResult> GetByReviewer(Guid reviewerId)
    {
        return await HandleAsync(() => _service.GetByReviewerAsync(reviewerId));
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByPeriod(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByPeriodAsync(startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePerformanceReviewDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePerformanceReviewDto dto)
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