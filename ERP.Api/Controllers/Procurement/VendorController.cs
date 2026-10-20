using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs;
using ERP.Shared.DTOs.Procurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Procurement;

[ApiController]
[Route("api/procurement/vendors")]
[Authorize]
[PermissionResource("procurement.vendors")]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
public class VendorController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService, ILogger<VendorController> logger) : base(logger)
    {
        _vendorService = vendorService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _vendorService.AddVendorAsync(dto, tenantId, userId);
        return CreatedAtAction(nameof(GetById), new { id = result }, new { VendorId = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var vendor = await _vendorService.GetVendorByIdAsync(id, tenantId);
        if (vendor == null)
            return NotFound();
        return Ok(vendor);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] string? category = null, [FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var query = new VendorQuery
        {
            TenantId = tenantId,
            SearchTerm = searchTerm,
            IsActive = isActive
        };
        var (vendors, totalCount) = await _vendorService.SearchVendorsAsync(query);
        return Ok(new { Vendors = vendors, TotalCount = totalCount });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorDto dto)
    {
        dto.VendorId = id;
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var result = await _vendorService.UpdateVendorAsync(dto, tenantId, userId);
        return Ok(new { Success = result != Guid.Empty });
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _vendorService.ApproveAsync(id, GetTenantId(), GetUserId());
        if (!ok)
            return NotFound();
        return Ok(new MessageResponse { Message = "Vendor activated" });
    }

    [HttpPost("{id}/suspend")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendVendorDto? dto = null)
    {
        var ok = await _vendorService.SuspendAsync(
            id,
            dto ?? new SuspendVendorDto { VendorId = id },
            GetTenantId(),
            GetUserId());
        if (!ok)
            return NotFound();
        return Ok(new MessageResponse { Message = "Vendor suspended" });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _vendorService.DeleteVendorAsync(id, tenantId);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("{vendorId:guid}/bank-accounts")]
    [ProducesResponseType(typeof(IEnumerable<VendorBankAccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBankAccounts(Guid vendorId)
        => Ok(await _vendorService.GetVendorBankAccountsAsync(vendorId, GetTenantId()));

    [HttpPost("{vendorId:guid}/bank-accounts")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBankAccount(Guid vendorId, [FromBody] CreateVendorBankAccountDto dto)
    {
        try
        {
            var id = await _vendorService.AddVendorBankAccountAsync(dto, vendorId, GetTenantId(), GetUserId());
            if (id == Guid.Empty) return BadRequest(new { error = "Unable to create bank account." });
            return CreatedAtAction(nameof(ListBankAccounts), new { vendorId }, new { id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{vendorId:guid}/bank-accounts/{bankAccountId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBankAccount(Guid vendorId, Guid bankAccountId, [FromBody] UpdateVendorBankAccountDto dto)
    {
        dto.Id = bankAccountId;
        try
        {
            var id = await _vendorService.UpdateVendorBankAccountAsync(dto, GetTenantId(), GetUserId());
            var accounts = await _vendorService.GetVendorBankAccountsAsync(vendorId, GetTenantId());
            var row = accounts.FirstOrDefault(a => a.Id == id);
            return row == null ? NotFound() : Ok(row);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{vendorId:guid}/bank-accounts/{bankAccountId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBankAccount(Guid vendorId, Guid bankAccountId)
    {
        var accounts = await _vendorService.GetVendorBankAccountsAsync(vendorId, GetTenantId());
        if (!accounts.Any(a => a.Id == bankAccountId)) return NotFound();
        var ok = await _vendorService.DeleteVendorBankAccountAsync(bankAccountId, GetTenantId());
        return ok ? NoContent() : NotFound();
    }

    private Guid GetTenantId() => HttpContext.RequireTenantId();

    private Guid GetUserId() => User.RequireUserId();
}
