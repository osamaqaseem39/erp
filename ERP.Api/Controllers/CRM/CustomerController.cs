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
[Route("api/crm/customers")]
public class CustomerController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICustomerService _service;
    private readonly CreateCustomerValidator _createValidator;
    private readonly UpdateCustomerValidator _updateValidator;

    public CustomerController(
        ICustomerService service,
        CreateCustomerValidator createValidator,
        UpdateCustomerValidator updateValidator,
        ILogger<CustomerController> logger) : base(logger)
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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        return await HandleAsync(() => _service.SearchAsync(searchTerm));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpGet("industry/{industry}")]
    public async Task<IActionResult> GetByIndustry(string industry)
    {
        return await HandleAsync(() => _service.GetByIndustryAsync(industry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = validationResult.Errors });
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
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
