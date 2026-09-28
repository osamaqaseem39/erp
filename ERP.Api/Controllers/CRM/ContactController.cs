using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/contacts")]
public class ContactController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IContactService _service;
    private readonly CreateContactValidator _createValidator;
    private readonly UpdateContactValidator _updateValidator;

    public ContactController(
        IContactService service,
        CreateContactValidator createValidator,
        UpdateContactValidator updateValidator,
        ILogger<ContactController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto)
    {
        try
        {
            dto.Id = id;
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await _service.UpdateAsync(dto);
            return result ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        try
        {
            var result = await _service.GetByCustomerAsync(customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        try
        {
            var result = await _service.SearchAsync(searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}