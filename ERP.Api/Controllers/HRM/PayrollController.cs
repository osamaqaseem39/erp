using Microsoft.AspNetCore.Mvc;
using ERP.Api.Abstractions;
using ERP.Api.Services;
using ERP.Shared.DTOs.HRM;
using FluentValidation;
using ERP.Api.Validators.HRM;
using ERP.Application.Interfaces.HRM;

namespace ERP.Api.Controllers.HRM;

[ApiController]
[Route("api/hrm/payroll")]
public class PayrollController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(
        IPayrollService service,
        ILogger<PayrollController> logger) : base(logger)
    {
        _service = service;
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
    public async Task<IActionResult> GetByPeriod(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByPeriodAsync(startDate, endDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PayrollDto dto)
    {
        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PayrollDto dto)
    {
        dto.Id = id;
        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id));
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(Guid id)
    {
        return await HandleAsync(() => _service.ProcessPayrollAsync(id));
    }
}