using ERP.Api.Errors;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.CRM;
using ERP.Api.Validators.CRM;

namespace ERP.Api.Controllers.CRM;

[Authorize]
[ApiController]
[Route("api/crm/sales-orders")]
[PermissionResource("crm.salesOrders")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SalesOrderController : ERP.Api.Abstractions.ControllerBase
{
    private readonly ISalesOrderService _service;
    private readonly CreateSalesOrderValidator _createValidator;
    private readonly UpdateSalesOrderValidator _updateValidator;

    public SalesOrderController(
        ISalesOrderService service,
        CreateSalesOrderValidator createValidator,
        UpdateSalesOrderValidator updateValidator,
        ILogger<SalesOrderController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SalesOrderDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<SalesOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        return await HandleAsync(() => _service.GetByCustomerAsync(customerId));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<SalesOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return await HandleAsync(() => _service.GetByStatusAsync(status));
    }

    [HttpGet("period")]
    [ProducesResponseType(typeof(IEnumerable<SalesOrderDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return await HandleAsync(() => _service.GetByDateRangeAsync(startDate, endDate));
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesOrderDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.UpdateAsync(dto));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id));
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _service.ApproveAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to approve sales order (status, tenant, or creator cannot approve)" });
        return Ok(new MessageResponse { Message = "Sales order approved successfully" });
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var ok = await _service.RejectAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to reject sales order" });
        return Ok(new MessageResponse { Message = "Sales order rejected successfully" });
    }

    [HttpPost("{id}/ship")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Ship(Guid id)
    {
        var ok = await _service.ShipAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to ship sales order (must be Approved; check stock)" });
        return Ok(new MessageResponse { Message = "Sales order shipped successfully" });
    }

    [HttpPost("{id}/invoice")]
    [ProducesResponseType(typeof(MessageResponse), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Invoice(Guid id)
    {
        var ok = await _service.InvoiceAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to invoice sales order (must be Shipped)" });
        return Ok(new MessageResponse { Message = "Sales order invoiced successfully" });
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
