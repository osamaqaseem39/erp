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
[Route("api/inventory/putaway-tasks")]
[Authorize]
[PermissionResource("inventory.putaway")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PutawayTaskController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPutawayTaskService _service;
    public PutawayTaskController(IPutawayTaskService service, ILogger<PutawayTaskController> logger) : base(logger)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePutawayTaskDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id) =>
        await _service.StartAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePutawayTaskDto? dto = null) =>
        await _service.CompleteAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId())
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
[Route("api/inventory/pick-lists")]
[Authorize]
[PermissionResource("inventory.pick")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PickListController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPickListService _service;
    public PickListController(IPickListService service, ILogger<PickListController> logger) : base(logger)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreatePickListDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("from-dispatch/{dispatchId:guid}")]
    [ProducesResponseType(typeof(IdResponse), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateFromDispatch(
        Guid dispatchId,
        [FromServices] ERP.Application.Interfaces.Sales.IDispatchAdviceService dispatch)
    {
        var tenantId = HttpContext.RequireTenantId();
        var userId = User.RequireUserId();
        var advice = await dispatch.GetByIdAsync(dispatchId, tenantId);
        if (advice == null) return NotFound();
        if (!advice.WarehouseId.HasValue || advice.WarehouseId == Guid.Empty) return BadRequest();
        var lines = advice.Lines
            .Where(l => l.ItemId != Guid.Empty && l.Quantity > 0)
            .Select(l => (l.ItemId, l.Quantity));
        var id = await _service.CreateFromDispatchAsync(
            dispatchId, advice.WarehouseId.Value, lines, tenantId, userId);
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id) =>
        await _service.StartAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePickListDto? dto = null) =>
        await _service.CompleteAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId())
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
[Route("api/inventory/replenishment-tasks")]
[Authorize]
[PermissionResource("inventory.replenishment")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ReplenishmentTaskController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IReplenishmentTaskService _service;
    public ReplenishmentTaskController(IReplenishmentTaskService service, ILogger<ReplenishmentTaskController> logger) : base(logger)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReplenishmentTaskDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id) =>
        await _service.StartAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id) =>
        await _service.CompleteAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
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
[Route("api/inventory/pack-orders")]
[Authorize]
[PermissionResource("inventory.pack")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PackOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPackOrderService _service;
    public PackOrderController(IPackOrderService service, ILogger<PackOrderController> logger) : base(logger)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackOrderDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("from-pick/{pickListId:guid}")]
    public async Task<IActionResult> CreateFromPick(Guid pickListId)
    {
        var id = await _service.CreateFromPickAsync(pickListId, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id) =>
        await _service.CompleteAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
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
[Route("api/inventory/shipments")]
[Authorize]
[PermissionResource("inventory.shipments")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ShipmentController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IShipmentService _service;
    public ShipmentController(IShipmentService service, ILogger<ShipmentController> logger) : base(logger)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentDto dto)
    {
        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("from-pack/{packOrderId:guid}")]
    public async Task<IActionResult> CreateFromPack(Guid packOrderId)
    {
        var id = await _service.CreateFromPackAsync(packOrderId, HttpContext.RequireTenantId(), User.RequireUserId());
        return id == Guid.Empty ? BadRequest() : CreatedAtAction(nameof(GetById), new { id }, new IdResponse { Id = id });
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, [FromBody] ShipShipmentDto? dto = null) =>
        await _service.ShipAsync(id, dto, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id) =>
        await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId())
            ? Ok(new SuccessResponse { Success = true }) : BadRequest();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id, HttpContext.RequireTenantId()) ? NoContent() : NotFound();
}
