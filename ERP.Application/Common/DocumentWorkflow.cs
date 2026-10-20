namespace ERP.Application.Common;

/// <summary>
/// Shared status-transition and approval guards (docs/operating ERP-FIX-5 / ERP-FIX-6).
/// </summary>
public static class DocumentWorkflow
{
    public static bool CanTransition(
        string? fromStatus,
        string toStatus,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> allowed)
    {
        var from = Normalize(fromStatus);
        var to = Normalize(toStatus);
        if (!allowed.TryGetValue(from, out var next)) return false;
        return next.Any(s => string.Equals(s, to, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsSameActor(string? createdBy, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(createdBy)) return false;
        if (Guid.TryParse(createdBy, out var createdGuid))
            return createdGuid == actorId;
        return string.Equals(createdBy, actorId.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "Draft" : status.Trim();
}
