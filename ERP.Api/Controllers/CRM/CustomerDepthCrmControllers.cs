using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Application.CRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/pouch-types")]
public class PouchTypeController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IPouchTypeService _service;
    public PouchTypeController(IPouchTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePouchTypeDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePouchTypeDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Updated" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId())
            ? Ok(new MessageResponse { Message = "Deactivated" }) : NotFound();
}

[Authorize]
[ApiController]
[Route("api/crm/production-processes")]
public class ProductionProcessController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IProductionProcessService _service;
    public ProductionProcessController(IProductionProcessService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionProcessDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionProcessDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Updated" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId())
            ? Ok(new MessageResponse { Message = "Deactivated" }) : NotFound();
}

[Authorize]
[ApiController]
[Route("api/crm/foh-types")]
public class FOHTypeController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IFOHTypeService _service;
    public FOHTypeController(IFOHTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFOHTypeDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFOHTypeDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Updated" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId())
            ? Ok(new MessageResponse { Message = "Deactivated" }) : NotFound();
}

[Authorize]
[ApiController]
[Route("api/crm/sale-enquiries")]
public class SaleEnquiryController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly ISaleEnquiryService _service;
    public SaleEnquiryController(ISaleEnquiryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleEnquiryDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleEnquiryDto dto)
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

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id) =>
        await _service.RejectAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Rejected" }) : NotFound();
}

[Authorize]
[ApiController]
[Route("api/crm/sale-pre-costs")]
public class SalePreCostController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly ISalePreCostService _service;
    public SalePreCostController(ISalePreCostService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalePreCostDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalePreCostDto dto)
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

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id) =>
        await _service.RejectAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new MessageResponse { Message = "Rejected" }) : NotFound();
}
