using ERP.Api.Errors;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.OpenApi;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/banks")]
[Authorize]
[PermissionResource("finance.banks")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class BankController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IBankService _service;
    private readonly CreateBankValidator _createValidator;
    private readonly UpdateBankValidator _updateValidator;

    public BankController(
        IBankService service,
        CreateBankValidator createValidator,
        UpdateBankValidator updateValidator,
        ILogger<BankController> logger) : base(logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BankDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var bank = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return bank == null ? NotFound() : Ok(bank);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BankDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var tenantId = HttpContext.RequireTenantId();
        var banks = activeOnly
            ? await _service.GetActiveAsync(tenantId)
            : await _service.GetAllAsync(tenantId);
        return Ok(banks);
    }

    [HttpGet("{id}/accounts")]
    [ProducesResponseType(typeof(IEnumerable<BankAccountDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetBankAccounts(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        if (await _service.GetByIdAsync(id, tenantId) == null)
            return NotFound();
        return Ok(await _service.GetBankAccountsAsync(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BankDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Create([FromBody] CreateBankDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        var id = await _service.CreateAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        if (id == Guid.Empty) return BadRequest(new { Error = "Bank create failed." });
        var created = await _service.GetByIdAsync(id, HttpContext.RequireTenantId());
        return Ok(created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BankDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankDto dto)
    {
        dto.Id = id;
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        var tenantId = HttpContext.RequireTenantId();
        var ok = await _service.UpdateAsync(dto, tenantId, User.RequireUserId());
        if (!ok) return NotFound();
        return Ok(await _service.GetByIdAsync(id, tenantId));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, HttpContext.RequireTenantId());
        return ok ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("accounts")]
    [ProducesResponseType(typeof(BankAccountDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CreateBankAccount([FromBody] CreateBankAccountDto dto)
    {
        return await HandleAsync(() => _service.CreateBankAccountAsync(dto));
    }

    [HttpPut("accounts/{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UpdateBankAccount(Guid id, [FromBody] UpdateBankAccountDto dto)
    {
        dto.Id = id;
        return await HandleAsync(() => _service.UpdateBankAccountAsync(dto));
    }

    [HttpDelete("accounts/{id}")]
    [ProducesResponseType(typeof(bool), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> DeleteBankAccount(Guid id)
    {
        return await HandleAsync(() => _service.DeleteBankAccountAsync(id));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IEnumerable<BankTransactionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListTransactions(
        [FromQuery] Guid? bankAccountId = null,
        [FromQuery] bool? unreconciledOnly = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await _service.ListBankTransactionsAsync(
            HttpContext.RequireTenantId(), bankAccountId, unreconciledOnly, toDate);
        return Ok(result);
    }

    [HttpGet("accounts/{accountId:guid}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<BankTransactionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetAccountTransactions(
        Guid accountId,
        [FromQuery] bool? unreconciledOnly = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await _service.GetBankTransactionsAsync(
            accountId, HttpContext.RequireTenantId(), unreconciledOnly, toDate);
        return Ok(result);
    }

    [HttpPost("accounts/{accountId:guid}/transactions")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> AddAccountTransaction(Guid accountId, [FromBody] CreateBankTransactionDto dto)
    {
        dto.BankAccountId = accountId;
        var id = await _service.AddBankTransactionAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return id != Guid.Empty ? Ok(new { id }) : BadRequest(new { error = "Unable to add bank transaction." });
    }

    [HttpPost("transactions/{bankTxId:guid}/match")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MatchTransaction(Guid bankTxId, [FromBody] BankTransactionMatchDto dto)
    {
        dto.BankTransactionId = bankTxId;
        var ok = await _service.MatchBankTransactionToPaymentAsync(
            dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok ? Ok(new { matched = true }) : BadRequest(new { error = "Unable to match payment." });
    }

    [HttpPost("transactions/{bankTxId:guid}/unmatch")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UnmatchTransaction(Guid bankTxId)
    {
        var ok = await _service.UnmatchBankTransactionAsync(
            bankTxId, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok ? Ok(new { matched = false }) : BadRequest(new { error = "Unable to unmatch." });
    }

    [HttpGet("accounts/{accountId:guid}/reconciliations")]
    [ProducesResponseType(typeof(IEnumerable<BankReconciliationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListReconciliations(Guid accountId, [FromQuery] string? status = null)
    {
        return Ok(await _service.ListReconciliationsAsync(accountId, HttpContext.RequireTenantId(), status));
    }

    [HttpGet("reconciliations/{id:guid}")]
    [ProducesResponseType(typeof(BankReconciliationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetReconciliation(Guid id)
    {
        var row = await _service.GetReconciliationAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("reconciliations")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateReconciliation([FromBody] CreateBankReconciliationDto dto)
    {
        var id = await _service.CreateReconciliationAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        if (id == Guid.Empty)
            return BadRequest(new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = "Could not open reconciliation (account missing or another open recon exists)."
            });
        return CreatedAtAction(nameof(GetReconciliation), new { id }, new { id });
    }

    [HttpPost("reconciliations/{id:guid}/transactions/{bankTxId:guid}/reconcile")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> MarkReconciled(Guid id, Guid bankTxId)
    {
        var ok = await _service.SetTransactionReconciledAsync(
            id, bankTxId, true, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok ? Ok(new { reconciled = true }) : BadRequest(new { error = "Unable to reconcile transaction." });
    }

    [HttpPost("reconciliations/{id:guid}/transactions/{bankTxId:guid}/unreconcile")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> UnmarkReconciled(Guid id, Guid bankTxId)
    {
        var ok = await _service.SetTransactionReconciledAsync(
            id, bankTxId, false, HttpContext.RequireTenantId(), User.RequireUserId());
        return ok ? Ok(new { reconciled = false }) : BadRequest(new { error = "Unable to unreconcile transaction." });
    }

    [HttpPost("reconciliations/{id:guid}/close")]
    [ProducesResponseType(typeof(BankReconciliationDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> CloseReconciliation(Guid id)
    {
        var ok = await _service.CloseReconciliationAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        if (!ok) return BadRequest(new { error = "Could not close — difference must be zero and status Open." });
        return Ok(await _service.GetReconciliationAsync(id, HttpContext.RequireTenantId()));
    }
}
