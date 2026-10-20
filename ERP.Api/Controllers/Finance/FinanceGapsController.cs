using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance")]
[Authorize]
[PermissionResource("finance.gaps")]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class FinanceGapsController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IFinanceGapService _service;

    public FinanceGapsController(IFinanceGapService service, ILogger<FinanceGapsController> logger) : base(logger) =>
        _service = service;

    [HttpGet("budgets/{budgetId:guid}/lines")]
    [ProducesResponseType(typeof(IEnumerable<BudgetLineDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListBudgetLines(Guid budgetId) =>
        Ok(await _service.ListBudgetLinesAsync(budgetId, HttpContext.RequireTenantId()));

    [HttpPost("budgets/lines")]
    [ProducesResponseType(typeof(BudgetLineDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> AddBudgetLine([FromBody] CreateBudgetLineDto dto)
    {
        var row = await _service.AddBudgetLineAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/budgets/{row.BudgetId}/lines", row);
    }

    [HttpGet("tax-groups")]
    [ProducesResponseType(typeof(IEnumerable<TaxGroupDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListTaxGroups() =>
        Ok(await _service.ListTaxGroupsAsync(HttpContext.RequireTenantId()));

    [HttpGet("tax-groups/{id:guid}")]
    [ProducesResponseType(typeof(TaxGroupDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTaxGroup(Guid id)
    {
        var row = await _service.GetTaxGroupAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("tax-groups")]
    [ProducesResponseType(typeof(TaxGroupDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateTaxGroup([FromBody] CreateTaxGroupDto dto)
    {
        var row = await _service.CreateTaxGroupAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetTaxGroup), new { id = row.Id }, row);
    }

    [HttpGet("asset-categories")]
    [ProducesResponseType(typeof(IEnumerable<AssetCategoryDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListAssetCategories() =>
        Ok(await _service.ListAssetCategoriesAsync(HttpContext.RequireTenantId()));

    [HttpPost("asset-categories")]
    [ProducesResponseType(typeof(AssetCategoryDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateAssetCategory([FromBody] CreateAssetCategoryDto dto)
    {
        var row = await _service.CreateAssetCategoryAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/asset-categories/{row.Id}", row);
    }

    [HttpGet("asset-disposals")]
    [ProducesResponseType(typeof(IEnumerable<AssetDisposalDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListDisposals() =>
        Ok(await _service.ListAssetDisposalsAsync(HttpContext.RequireTenantId()));

    [HttpGet("asset-disposals/{id:guid}")]
    [ProducesResponseType(typeof(AssetDisposalDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDisposal(Guid id)
    {
        var row = await _service.GetAssetDisposalAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("asset-disposals")]
    [ProducesResponseType(typeof(AssetDisposalDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateDisposal([FromBody] CreateAssetDisposalDto dto)
    {
        var row = await _service.CreateAssetDisposalAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetDisposal), new { id = row.Id }, row);
    }

    [HttpGet("asset-transfers")]
    [ProducesResponseType(typeof(IEnumerable<AssetTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListTransfers() =>
        Ok(await _service.ListAssetTransfersAsync(HttpContext.RequireTenantId()));

    [HttpGet("asset-transfers/{id:guid}")]
    [ProducesResponseType(typeof(AssetTransferDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var row = await _service.GetAssetTransferAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("asset-transfers")]
    [ProducesResponseType(typeof(AssetTransferDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateAssetTransferDto dto)
    {
        var row = await _service.CreateAssetTransferAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetTransfer), new { id = row.Id }, row);
    }

    [HttpGet("fund-transfers")]
    [ProducesResponseType(typeof(IEnumerable<FundTransferDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListFundTransfers() =>
        Ok(await _service.ListFundTransfersAsync(HttpContext.RequireTenantId()));

    [HttpGet("fund-transfers/{id:guid}")]
    [ProducesResponseType(typeof(FundTransferDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetFundTransfer(Guid id)
    {
        var row = await _service.GetFundTransferAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("fund-transfers")]
    [ProducesResponseType(typeof(FundTransferDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateFundTransfer([FromBody] CreateFundTransferDto dto)
    {
        var row = await _service.CreateFundTransferAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/fund-transfers/{row.Id}", row);
    }

    [HttpPost("fund-transfers/{id:guid}/post")]
    [ProducesResponseType(typeof(FundTransferDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostFundTransfer(Guid id) =>
        Ok(await _service.PostFundTransferAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpGet("credit-memos")]
    [ProducesResponseType(typeof(IEnumerable<CreditMemoDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListCreditMemos() =>
        Ok(await _service.ListCreditMemosAsync(HttpContext.RequireTenantId()));

    [HttpGet("credit-memos/{id:guid}")]
    [ProducesResponseType(typeof(CreditMemoDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCreditMemo(Guid id)
    {
        var row = await _service.GetCreditMemoAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("credit-memos")]
    [ProducesResponseType(typeof(CreditMemoDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateCreditMemo([FromBody] CreateCreditMemoDto dto)
    {
        var row = await _service.CreateCreditMemoAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/credit-memos/{row.Id}", row);
    }

    [HttpPost("credit-memos/{id:guid}/post")]
    [ProducesResponseType(typeof(CreditMemoDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostCreditMemo(Guid id) =>
        Ok(await _service.PostCreditMemoAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpGet("debit-memos")]
    [ProducesResponseType(typeof(IEnumerable<DebitMemoDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListDebitMemos() =>
        Ok(await _service.ListDebitMemosAsync(HttpContext.RequireTenantId()));

    [HttpGet("debit-memos/{id:guid}")]
    [ProducesResponseType(typeof(DebitMemoDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetDebitMemo(Guid id)
    {
        var row = await _service.GetDebitMemoAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("debit-memos")]
    [ProducesResponseType(typeof(DebitMemoDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateDebitMemo([FromBody] CreateDebitMemoDto dto)
    {
        var row = await _service.CreateDebitMemoAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/debit-memos/{row.Id}", row);
    }

    [HttpPost("debit-memos/{id:guid}/post")]
    [ProducesResponseType(typeof(DebitMemoDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PostDebitMemo(Guid id) =>
        Ok(await _service.PostDebitMemoAsync(id, HttpContext.RequireTenantId(), User.RequireUserId()));

    [HttpGet("customer-advances")]
    [ProducesResponseType(typeof(IEnumerable<CustomerAdvanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListCustomerAdvances() =>
        Ok(await _service.ListCustomerAdvancesAsync(HttpContext.RequireTenantId()));

    [HttpGet("customer-advances/{id:guid}")]
    [ProducesResponseType(typeof(CustomerAdvanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetCustomerAdvance(Guid id)
    {
        var row = await _service.GetCustomerAdvanceAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("customer-advances")]
    [ProducesResponseType(typeof(CustomerAdvanceDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateCustomerAdvance([FromBody] CreateCustomerAdvanceDto dto)
    {
        var row = await _service.CreateCustomerAdvanceAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/customer-advances/{row.Id}", row);
    }

    [HttpGet("vendor-advances")]
    [ProducesResponseType(typeof(IEnumerable<VendorAdvanceDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListVendorAdvances() =>
        Ok(await _service.ListVendorAdvancesAsync(HttpContext.RequireTenantId()));

    [HttpGet("vendor-advances/{id:guid}")]
    [ProducesResponseType(typeof(VendorAdvanceDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetVendorAdvance(Guid id)
    {
        var row = await _service.GetVendorAdvanceAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("vendor-advances")]
    [ProducesResponseType(typeof(VendorAdvanceDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateVendorAdvance([FromBody] CreateVendorAdvanceDto dto)
    {
        var row = await _service.CreateVendorAdvanceAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/vendor-advances/{row.Id}", row);
    }

    [HttpGet("advance-applications")]
    [ProducesResponseType(typeof(IEnumerable<AdvanceApplicationDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListAdvanceApplications() =>
        Ok(await _service.ListAdvanceApplicationsAsync(HttpContext.RequireTenantId()));

    [HttpPost("advance-applications")]
    [ProducesResponseType(OpenApiResponseTypes.Created)]
    public async Task<IActionResult> ApplyAdvance([FromBody] CreateAdvanceApplicationDto dto)
    {
        var id = await _service.ApplyAdvanceAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/advance-applications/{id}", new { id });
    }

    [HttpGet("withholding-tax-lines")]
    [ProducesResponseType(typeof(IEnumerable<WithholdingTaxLineDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListWithholding() =>
        Ok(await _service.ListWithholdingTaxLinesAsync(HttpContext.RequireTenantId()));

    [HttpPost("withholding-tax-lines")]
    [ProducesResponseType(typeof(WithholdingTaxLineDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> AddWithholding([FromBody] CreateWithholdingTaxLineDto dto)
    {
        var row = await _service.AddWithholdingAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/withholding-tax-lines/{row.Id}", row);
    }

    [HttpGet("recurring-journals")]
    [ProducesResponseType(typeof(IEnumerable<RecurringJournalDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListRecurring() =>
        Ok(await _service.ListRecurringJournalsAsync(HttpContext.RequireTenantId()));

    [HttpGet("recurring-journals/{id:guid}")]
    [ProducesResponseType(typeof(RecurringJournalDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetRecurring(Guid id)
    {
        var row = await _service.GetRecurringJournalAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("recurring-journals")]
    [ProducesResponseType(typeof(RecurringJournalDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringJournalDto dto)
    {
        var row = await _service.CreateRecurringJournalAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/recurring-journals/{row.Id}", row);
    }

    [HttpPost("recurring-journals/{id:guid}/generate")]
    [ProducesResponseType(OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GenerateRecurring(Guid id)
    {
        var journalId = await _service.GenerateRecurringJournalAsync(id, HttpContext.RequireTenantId(), User.RequireUserId());
        return Ok(new { journalId });
    }

    [HttpGet("fx-revaluations")]
    [ProducesResponseType(typeof(IEnumerable<FxRevaluationRunDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListFxRuns() =>
        Ok(await _service.ListFxRunsAsync(HttpContext.RequireTenantId()));

    [HttpGet("fx-revaluations/{id:guid}")]
    [ProducesResponseType(typeof(FxRevaluationRunDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetFxRun(Guid id)
    {
        var row = await _service.GetFxRunAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("fx-revaluations")]
    [ProducesResponseType(typeof(FxRevaluationRunDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateFxRun([FromBody] CreateFxRevaluationRunDto dto)
    {
        var row = await _service.CreateFxRunAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/fx-revaluations/{row.Id}", row);
    }

    [HttpGet("intercompany-settlements")]
    [ProducesResponseType(typeof(IEnumerable<IntercompanySettlementDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListIcSettlements() =>
        Ok(await _service.ListIcSettlementsAsync(HttpContext.RequireTenantId()));

    [HttpGet("intercompany-settlements/{id:guid}")]
    [ProducesResponseType(typeof(IntercompanySettlementDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetIcSettlement(Guid id)
    {
        var row = await _service.GetIcSettlementAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("intercompany-settlements")]
    [ProducesResponseType(typeof(IntercompanySettlementDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateIcSettlement([FromBody] CreateIntercompanySettlementDto dto)
    {
        var row = await _service.CreateIcSettlementAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return Created($"api/finance/intercompany-settlements/{row.Id}", row);
    }

    [HttpGet("intercompany-transactions")]
    [ProducesResponseType(typeof(IEnumerable<IntercompanyTransactionDto>), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> ListIcTransactions([FromQuery] bool unsettledOnly = false) =>
        Ok(await _service.ListIcTransactionsAsync(HttpContext.RequireTenantId(), unsettledOnly));

    [HttpGet("intercompany-transactions/{id:guid}")]
    [ProducesResponseType(typeof(IntercompanyTransactionDto), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> GetIcTransaction(Guid id)
    {
        var row = await _service.GetIcTransactionAsync(id, HttpContext.RequireTenantId());
        return row == null ? NotFound() : Ok(row);
    }

    [HttpPost("intercompany-transactions")]
    [ProducesResponseType(typeof(IntercompanyTransactionDto), OpenApiResponseTypes.Created)]
    public async Task<IActionResult> CreateIcTransaction([FromBody] CreateIntercompanyTransactionDto dto)
    {
        var row = await _service.CreateIcTransactionAsync(dto, HttpContext.RequireTenantId(), User.RequireUserId());
        return CreatedAtAction(nameof(GetIcTransaction), new { id = row.Id }, row);
    }
}
