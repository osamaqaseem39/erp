namespace ERP.Shared.Abstractions;

/// <summary>Tenant-scoped binary store for project document files.</summary>
public interface IProjectDocumentStorage
{
    /// <summary>Persists content and returns a relative path safe to store on the entity.</summary>
    Task<string> SaveAsync(Guid tenantId, Guid documentId, string fileName, byte[] content, CancellationToken ct = default);

    /// <summary>Reads bytes for a relative path belonging to the tenant; null if missing.</summary>
    Task<byte[]?> ReadAsync(Guid tenantId, string relativePath, CancellationToken ct = default);

    Task DeleteAsync(Guid tenantId, string relativePath, CancellationToken ct = default);
}
