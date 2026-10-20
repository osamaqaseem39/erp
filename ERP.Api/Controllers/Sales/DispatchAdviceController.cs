using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/dispatch-advices")]
[Authorize]
[PermissionResource("sales.dispatchAdvices")]
public class DispatchAdviceController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IDispatchAdviceService _service;

    public DispatchAdviceController(IDispatchAdviceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDispatchAdviceDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDispatchAdviceDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Updated" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId())
            ? Ok(new MessageResponse { Message = "Deleted" }) : NotFound();

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id) =>
        await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Approved" }) : NotFound();

    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid id) =>
        await _service.DispatchAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Dispatched" }) : NotFound();

    [HttpPost("{id:guid}/create-pick")]
    public async Task<IActionResult> CreatePick(Guid id)
    {
        var pickId = await _service.CreatePickAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return pickId == Guid.Empty
            ? BadRequest()
            : Ok(new IdResponse { Id = pickId });
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id) =>
        await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Cancelled" }) : NotFound();
}
