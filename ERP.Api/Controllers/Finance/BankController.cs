using ERP.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Api.Abstractions;
using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Api.Validators.Finance;
using FluentValidation;

namespace ERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/banks")]
[Authorize]
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
    public async Task<IActionResult> GetById(Guid id)
    {
        return await HandleAsync(() => _service.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(() => _service.GetAllAsync());
    }

    [HttpGet("{id}/accounts")]
    public async Task<IActionResult> GetBankAccounts(Guid id)
    {
        return await HandleAsync(() => _service.GetBankAccountsAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorMapper.ValidationError(validationResult.Errors));
        }

        return await HandleAsync(() => _service.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankDto dto)
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
    public async Task<IActionResult> Delete(Guid id)
    {
        return await HandleAsync(() => _service.DeleteAsync(id));
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateBankAccount([FromBody] CreateBankAccountDto dto)
    {
        return await HandleAsync(() => _service.CreateBankAccountAsync(dto));
    }

    [HttpPut("accounts/{id}")]
    public async Task<IActionResult> UpdateBankAccount(Guid id, [FromBody] UpdateBankAccountDto dto)
    {
        dto.Id = id;
        return await HandleAsync(() => _service.UpdateBankAccountAsync(dto));
    }

    [HttpDelete("accounts/{id}")]
    public async Task<IActionResult> DeleteBankAccount(Guid id)
    {
        return await HandleAsync(() => _service.DeleteBankAccountAsync(id));
    }
}