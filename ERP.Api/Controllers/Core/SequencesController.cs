using ERP.Api.OpenApi;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

/// <summary>Document numbering (prefix, digits, yearly reset, next number) for the current tenant.</summary>
[ApiController]
[Route("api/core/sequences")]
[Authorize]
[ProducesBadRequest]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class SequencesController : ControllerBase
{
    private readonly IDocumentNumberService _numbers;

    public SequencesController(IDocumentNumberService numbers)
    {
        _numbers = numbers;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentSequenceDto>), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<IReadOnlyList<DocumentSequenceDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _numbers.ListAsync(cancellationToken));

    [HttpPut("{key}")]
    [ProducesResponseType(typeof(DocumentSequenceDto), OpenApiResponseTypes.Ok)]
    public async Task<ActionResult<DocumentSequenceDto>> Update(string key, [FromBody] UpdateDocumentSequenceDto dto, CancellationToken cancellationToken) =>
        Ok(await _numbers.UpdateAsync(key, dto, cancellationToken));
}
