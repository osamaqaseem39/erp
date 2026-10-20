using ERP.Shared.Abstractions;



namespace ERP.Infrastructure.Storage;



/// <summary>Stores project document binaries under {root}/{tenantId}/{documentId}/{fileName}.</summary>

public sealed class LocalProjectDocumentStorage : IProjectDocumentStorage

{

    private readonly string _root;



    public LocalProjectDocumentStorage(string root)

    {

        _root = string.IsNullOrWhiteSpace(root)

            ? Path.Combine(Path.GetTempPath(), "erp-project-docs")

            : root;

        Directory.CreateDirectory(_root);

    }



    public async Task<string> SaveAsync(Guid tenantId, Guid documentId, string fileName, byte[] content, CancellationToken ct = default)

    {

        var safeName = SanitizeFileName(fileName);

        var relative = Path.Combine(tenantId.ToString("N"), documentId.ToString("N"), safeName);

        var full = ResolveUnderRoot(relative);

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await File.WriteAllBytesAsync(full, content, ct);

        return relative.Replace('\\', '/');

    }



    public async Task<byte[]?> ReadAsync(Guid tenantId, string relativePath, CancellationToken ct = default)

    {

        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        if (!BelongsToTenant(tenantId, relativePath)) return null;

        var full = ResolveUnderRoot(relativePath);

        if (!File.Exists(full)) return null;

        return await File.ReadAllBytesAsync(full, ct);

    }



    public Task DeleteAsync(Guid tenantId, string relativePath, CancellationToken ct = default)

    {

        if (string.IsNullOrWhiteSpace(relativePath) || !BelongsToTenant(tenantId, relativePath))

            return Task.CompletedTask;

        var full = ResolveUnderRoot(relativePath);

        if (File.Exists(full)) File.Delete(full);

        return Task.CompletedTask;

    }



    private string ResolveUnderRoot(string relativePath)

    {

        var combined = Path.GetFullPath(Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var rootFull = Path.GetFullPath(_root);

        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))

            throw new InvalidOperationException("Document path escapes storage root.");

        return combined;

    }



    private static bool BelongsToTenant(Guid tenantId, string relativePath)

    {

        var prefix = tenantId.ToString("N");

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');

        return normalized.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase)

               || normalized.StartsWith(prefix + "\\", StringComparison.OrdinalIgnoreCase)

               || normalized.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    }



    private static string SanitizeFileName(string fileName)

    {

        var name = Path.GetFileName(string.IsNullOrWhiteSpace(fileName) ? "document.bin" : fileName.Trim());

        foreach (var c in Path.GetInvalidFileNameChars())

            name = name.Replace(c, '_');

        return string.IsNullOrWhiteSpace(name) ? "document.bin" : name;

    }

}

