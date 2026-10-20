using System.Text;

namespace ERP.Application.Core;

/// <summary>
/// Minimal PDF writer (no third-party dependency) for SO/SI/PO/voucher prints.
/// Produces a valid single-page PDF with Helvetica text lines.
/// </summary>
public sealed class SimplePdfDocumentService : IPdfDocumentService
{
    public byte[] BuildSimpleDocument(string title, IReadOnlyList<string> lines)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 14 Tf");
        content.AppendLine("50 780 Td");
        content.AppendLine($"({Escape(title)}) Tj");
        content.AppendLine("/F1 10 Tf");
        content.AppendLine("0 -24 Td");
        foreach (var line in lines)
        {
            content.AppendLine($"({Escape(line)}) Tj");
            content.AppendLine("0 -14 Td");
        }
        content.AppendLine("ET");
        var stream = content.ToString();
        var streamBytes = Encoding.ASCII.GetBytes(stream);

        var objects = new List<byte[]>();
        objects.Add(Encoding.ASCII.GetBytes("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n"));
        objects.Add(Encoding.ASCII.GetBytes("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n"));
        objects.Add(Encoding.ASCII.GetBytes("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj\n"));
        objects.Add(Encoding.ASCII.GetBytes($"4 0 obj<< /Length {streamBytes.Length} >>stream\n").Concat(streamBytes).Concat(Encoding.ASCII.GetBytes("\nendstream\nendobj\n")).ToArray());
        objects.Add(Encoding.ASCII.GetBytes("5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n"));

        using var ms = new MemoryStream();
        var header = Encoding.ASCII.GetBytes("%PDF-1.4\n");
        ms.Write(header);
        var offsets = new List<long> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(ms.Position);
            ms.Write(obj);
        }
        var xrefPos = ms.Position;
        var xref = new StringBuilder();
        xref.AppendLine("xref");
        xref.AppendLine($"0 {objects.Count + 1}");
        xref.AppendLine("0000000000 65535 f ");
        for (var i = 1; i <= objects.Count; i++)
            xref.AppendLine($"{offsets[i]:D10} 00000 n ");
        xref.AppendLine("trailer<< /Size " + (objects.Count + 1) + " /Root 1 0 R >>");
        xref.AppendLine("startxref");
        xref.AppendLine(xrefPos.ToString());
        xref.AppendLine("%%EOF");
        var xrefBytes = Encoding.ASCII.GetBytes(xref.ToString());
        ms.Write(xrefBytes);
        return ms.ToArray();
    }

    private static string Escape(string text) =>
        (text ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", " ")
            .Replace("\n", " ");
}
