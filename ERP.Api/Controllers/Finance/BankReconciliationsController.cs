using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/bank-reconciliations")]
[Authorize]
[PermissionResource("finance.bankReconciliations")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BankReconciliationsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBankService _service;

    public BankReconciliationsController(IBankService service, ILogger<BankReconciliationsController> logger)
        : base(logger)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BankReconciliationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> List([FromQuery] Guid bankAccountId, [FromQuery] string? status = null)
    {
        if (bankAccountId == Guid.Empty)
            return BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.ValidationError, Message = "bankAccountId is required." });
        var result = await _service.ListReconciliationsAsync(bankAccountId, GetTenantId(), status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BankReconciliationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetReconciliationAsync(id, GetTenantId());
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> Create([FromBody] CreateBankReconciliationDto dto)
    {
        var id = await _service.CreateReconciliationAsync(dto, GetTenantId(), GetUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to open reconciliation (account missing or another open recon exists)."
            });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/transactions/{bankTxId:guid}/reconcile")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkReconciled(Guid id, Guid bankTxId)
    {
        var ok = await _service.SetTransactionReconciledAsync(id, bankTxId, true, GetTenantId(), GetUserId());
        return ok
            ? Ok(new { reconciled = true })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to reconcile transaction." });
    }

    [HttpPost("{id:guid}/transactions/{bankTxId:guid}/unreconcile")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UnmarkReconciled(Guid id, Guid bankTxId)
    {
        var ok = await _service.SetTransactionReconciledAsync(id, bankTxId, false, GetTenantId(), GetUserId());
        return ok
            ? Ok(new { reconciled = false })
            : BadRequest(new ApiError { Status = 400, ErrorCode = ApiErrorCodes.BusinessRule, Message = "Unable to unreconcile transaction." });
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(BankReconciliationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Close(Guid id)
    {
        var ok = await _service.CloseReconciliationAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Unable to close — difference must be zero and status Open."
            });
        var updated = await _service.GetReconciliationAsync(id, GetTenantId());
        return Ok(updated);
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();
    private Guid GetUserId() => User.RequireUserId();
}
