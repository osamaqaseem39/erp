using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/serial-numbers")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SerialNumberController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISerialNumberService _service;
    private readonly ILogger<SerialNumberController> _logger;

    public SerialNumberController(ISerialNumberService service, ILogger<SerialNumberController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("item/{itemId}")]
    [ProducesResponseType(typeof(IEnumerable<SerialNumberDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByItem(Guid itemId, [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByItemAsync(itemId, tenantId, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SerialNumberDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] SerialNumberDto dto)
    {
        var tenantId = GetTenantId();
        var createdBy = GetCurrentUser();
        var result = await _service.CreateAsync(dto, tenantId, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SerialNumberDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SerialNumberDto dto)
    {
        dto.Id = id;
        var tenantId = GetTenantId();
        var updatedBy = GetCurrentUser();
        var result = await _service.UpdateAsync(dto, tenantId, updatedBy);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var success = await _service.DeleteAsync(id, tenantId);
        return success ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private string GetCurrentUser() => User.RequireUserId().ToString();
}