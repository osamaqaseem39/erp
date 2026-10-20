using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

[ApiController]
[Route("api/core/prints")]
[Authorize]
[RequirePermission("reports.print.view")]
[ProducesUnauthorized]
[ProducesForbidden]
public class DocumentPrintController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPdfDocumentService _pdf;

    public DocumentPrintController(IPdfDocumentService pdf, ILogger<DocumentPrintController> logger) : base(logger) =>
        _pdf = pdf;

    public sealed class PrintRequest
    {
        public string Title { get; set; } = "Document";
        public List<string> Lines { get; set; } = [];
    }

    [HttpPost]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public IActionResult Print([FromBody] PrintRequest request)
    {
        _ = HttpContext.RequireTenantId();
        var bytes = _pdf.BuildSimpleDocument(request.Title, request.Lines);
        return File(bytes, "application/pdf", $"{Sanitize(request.Title)}.pdf");
    }

    private static string Sanitize(string title) =>
        string.Join("_", (title ?? "document").Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
}
