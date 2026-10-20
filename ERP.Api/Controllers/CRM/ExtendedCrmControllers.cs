using ERP.Api.Errors;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Api.Security;
using ERP.Application.CRM;
using ERP.Api.Validators.CRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/salesmen")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalesmanController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesmanService _service;
    private readonly CreateSalesmanValidator _createValidator;
    private readonly UpdateSalesmanValidator _updateValidator;

    public SalesmanController(
        ISalesmanService service,
        CreateSalesmanValidator createValidator,
        UpdateSalesmanValidator updateValidator,
        ILogger<SalesmanController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SalesmanDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesmanDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(SalesmanDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateSalesmanDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SalesmanDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesmanDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

[Authorize]
[ApiController]
[Route("api/crm/customer-groups")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class CustomerGroupController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ICustomerGroupService _service;
    private readonly CreateCustomerGroupValidator _createValidator;
    private readonly UpdateCustomerGroupValidator _updateValidator;

    public CustomerGroupController(
        ICustomerGroupService service,
        CreateCustomerGroupValidator createValidator,
        UpdateCustomerGroupValidator updateValidator,
        ILogger<CustomerGroupController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerGroupDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerGroupDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(CustomerGroupDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerGroupDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerGroupDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerGroupDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

[Authorize]
[ApiController]
[Route("api/crm/price-lists")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class PriceListController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPriceListService _service;
    private readonly CreatePriceListValidator _createValidator;
    private readonly UpdatePriceListValidator _updateValidator;

    public PriceListController(
        IPriceListService service,
        CreatePriceListValidator createValidator,
        UpdatePriceListValidator updateValidator,
        ILogger<PriceListController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PriceListDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PriceListDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(PriceListDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreatePriceListDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PriceListDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

[Authorize]
[ApiController]
[Route("api/crm/sales-territories")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalesTerritoryController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesTerritoryService _service;
    private readonly CreateSalesTerritoryValidator _createValidator;
    private readonly UpdateSalesTerritoryValidator _updateValidator;

    public SalesTerritoryController(
        ISalesTerritoryService service,
        CreateSalesTerritoryValidator createValidator,
        UpdateSalesTerritoryValidator updateValidator,
        ILogger<SalesTerritoryController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SalesTerritoryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id) =>
        await HandleAsync(() => _service.GetByIdAsync(id, GetTenantId()));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesTerritoryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll() =>
        await HandleAsync(() => _service.GetAllAsync(GetTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(SalesTerritoryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateSalesTerritoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.CreateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SalesTerritoryDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesTerritoryDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        return await HandleAsync(() => _service.UpdateAsync(dto, GetTenantId(), GetUserId()));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id) =>
        await HandleAsync(() => _service.DeleteAsync(id, GetTenantId()));

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}

[Authorize]
[ApiController]
[Route("api/crm/return-orders")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class ReturnOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IReturnOrderService _service;
    private readonly CreateReturnOrderValidator _createValidator;
    private readonly UpdateReturnOrderValidator _updateValidator;

    public ReturnOrderController(
        IReturnOrderService service,
        CreateReturnOrderValidator createValidator,
        UpdateReturnOrderValidator updateValidator,
        ILogger<ReturnOrderController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReturnOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync(HttpContext.RequireTenantId()));

    [HttpPost]
    [ProducesResponseType(typeof(ReturnOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateReturnOrderDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        var tenantId = HttpContext.RequireTenantId();
        var id = await _service.CreateAsync(dto, tenantId, User.RequireUserId());
        if (id == Guid.Empty) return BadRequest(new { Error = "Unable to create return order." });
        return Ok(await _service.GetByIdAsync(id, tenantId));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReturnOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReturnOrderDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(ApiErrorMapper.ValidationError(validation.Errors));
        var tenantId = HttpContext.RequireTenantId();
        if (!await _service.UpdateAsync(dto, tenantId, User.RequireUserId())) return NotFound();
        return Ok(await _service.GetByIdAsync(id, tenantId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, HttpContext.RequireTenantId());
        return ok ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok
            ? Ok(new MessageResponse { Message = "Approved" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to approve return." });
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ReturnOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var ok = await _service.CompleteAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to complete return (must be Approved; stock/credit failed)."
            });
        return Ok(await _service.GetByIdAsync(id, HttpContext.RequireTenantId()));
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var ok = await _service.CancelAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok
            ? Ok(new MessageResponse { Message = "Cancelled" })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to cancel return." });
    }
}
