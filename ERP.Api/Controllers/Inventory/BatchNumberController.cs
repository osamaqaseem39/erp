using ERP.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/batch-numbers")]
[Authorize]
public class BatchNumberController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBatchNumberService _service;
    private readonly ILogger<BatchNumberController> _logger;

    public BatchNumberController(IBatchNumberService service, ILogger<BatchNumberController> logger) : base(logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetByItem(Guid itemId)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByItemAsync(itemId, tenantId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _service.GetByIdAsync(id, tenantId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BatchNumberDto dto)
    {
        var tenantId = GetTenantId();
        var createdBy = GetCurrentUser();
        var result = await _service.CreateAsync(dto, tenantId, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BatchNumberDto dto)
    {
        dto.Id = id;
        var tenantId = GetTenantId();
        var updatedBy = GetCurrentUser();
        var result = await _service.UpdateAsync(dto, tenantId, updatedBy);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var success = await _service.DeleteAsync(id, tenantId);
        return success ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private string GetCurrentUser() => User.RequireUserId().ToString();
}