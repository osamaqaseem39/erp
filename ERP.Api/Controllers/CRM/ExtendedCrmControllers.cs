using ERP.Api.Abstractions;
using ERP.Application.CRM;
using ERP.Api.Validators.CRM;
using ERP.Shared.DTOs.CRM;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/customer-groups")]
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

    [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) => await HandleAsync(() => _service.GetByIdAsync(id));
    [HttpGet] public async Task<IActionResult> GetAll() => await HandleAsync(() => _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerGroupDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerGroupDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await HandleAsync(() => _service.DeleteAsync(id));
}

[Authorize]
[ApiController]
[Route("api/crm/price-lists")]
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

    [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) => await HandleAsync(() => _service.GetByIdAsync(id));
    [HttpGet] public async Task<IActionResult> GetAll() => await HandleAsync(() => _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriceListDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await HandleAsync(() => _service.DeleteAsync(id));
}

[Authorize]
[ApiController]
[Route("api/crm/sales-territories")]
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

    [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) => await HandleAsync(() => _service.GetByIdAsync(id));
    [HttpGet] public async Task<IActionResult> GetAll() => await HandleAsync(() => _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesTerritoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesTerritoryDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await HandleAsync(() => _service.DeleteAsync(id));
}

[Authorize]
[ApiController]
[Route("api/crm/return-orders")]
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

    [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) => await HandleAsync(() => _service.GetByIdAsync(id));
    [HttpGet] public async Task<IActionResult> GetAll() => await HandleAsync(() => _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnOrderDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReturnOrderDto dto)
    {
        dto.Id = id;
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(new { message = "Validation failed", errors = validation.Errors });
        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await HandleAsync(() => _service.DeleteAsync(id));
}
