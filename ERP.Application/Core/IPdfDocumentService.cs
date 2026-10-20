namespace ERP.Application.Core;

public interface IPdfDocumentService
{
    /// <summary>Builds a minimal PDF (text lines) for document prints. Returns PDF bytes.</summary>
    byte[] BuildSimpleDocument(string title, IReadOnlyList<string> lines);
}
