using ERP.Api.Abstractions;
using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/brands")]
[Authorize]
[PermissionResource("inventory.brands")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BrandController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBrandService _service;
    public BrandController(IBrandService service, ILogger<BrandController> logger) : base(logger) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null) =>
        Ok(await _service.GetAllAsync(HttpContext.RequireTenantId(), isActive));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
    {
        var created = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandDto dto)
    {
        dto.Id = id;
        var updated = await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/inventory/warehouse-locations")]
[Authorize]
[PermissionResource("inventory.locations")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class WarehouseLocationController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IWarehouseLocationService _service;
    public WarehouseLocationController(IWarehouseLocationService service, ILogger<WarehouseLocationController> logger) : base(logger) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByWarehouse([FromQuery] Guid warehouseId) =>
        Ok(await _service.GetByWarehouseAsync(warehouseId, HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseLocationDto dto)
    {
        var created = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseLocationDto dto)
    {
        dto.Id = id;
        var updated = await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/inventory/reservations")]
[Authorize]
[PermissionResource("inventory.reservations")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class StockReservationController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IStockReservationService _service;
    public StockReservationController(IStockReservationService service, ILogger<StockReservationController> logger) : base(logger) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? salesOrderId = null)
    {
        var tenantId = HttpContext.RequireTenantId();
        var items = salesOrderId.HasValue
            ? await _service.GetBySalesOrderAsync(salesOrderId.Value, tenantId)
            : await _service.GetAllAsync(tenantId);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockReservationDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/release")]
    public async Task<IActionResult> Release(Guid id) =>
        await _service.ReleaseAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id) =>
        await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/inventory/kits")]
[Authorize]
[PermissionResource("inventory.kits")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ItemKitController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IItemKitService _service;
    public ItemKitController(IItemKitService service, ILogger<ItemKitController> logger) : base(logger) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemKitDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemKitDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/inventory/cycle-counts")]
[Authorize]
[PermissionResource("inventory.cycleCounts")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CycleCountController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICycleCountService _service;
    public CycleCountController(ICycleCountService service, ILogger<CycleCountController> logger) : base(logger) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCycleCountDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCycleCountDto dto)
    {
        dto.Id = id;
        return await _service.UpdateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();
    }

    [HttpPost("{id:guid}/count")]
    public async Task<IActionResult> MarkCounted(Guid id) =>
        await _service.MarkCountedAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id) =>
        await _service.PostAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}
