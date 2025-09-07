using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using ERP.Api.Validators.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/training")]
public class TrainingController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ITrainingService _service;
    private readonly CreateTrainingValidator _createValidator;
    private readonly UpdateTrainingValidator _updateValidator;

    public TrainingController(
        ITrainingService service,
        CreateTrainingValidator createValidator,
        UpdateTrainingValidator updateValidator,
        ILogger<TrainingController> logger) : base(logger)
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

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTrainingDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainingDto dto)
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

    [HttpPost("{id}/enroll")]
    public async Task<IActionResult> EnrollEmployee(Guid id, [FromBody] EnrollEmployeeDto dto)
    {
        return await HandleAsync(() => _service.EnrollEmployeeAsync(id, dto.EmployeeId));
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTraining(Guid id, [FromBody] CompleteTrainingDto dto)
    {
        return await HandleAsync(() => _service.CompleteTrainingAsync(id, dto.EmployeeId, dto.Score ?? 0));
    }
}