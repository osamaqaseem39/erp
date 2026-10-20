using System.Text.RegularExpressions;
using ERP.Domain.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Numbering;

public sealed record SequenceDefault(string Key, string Label, string Prefix, int Padding = 5, string ResetPolicy = "Yearly");

/// <summary>Document types that get sequential numbers, with their out-of-the-box format.</summary>
public static class SequenceCatalog
{
    public static readonly IReadOnlyList<SequenceDefault> All =
    [
        new("crm.lead", "Leads", "LD"),
        new("crm.opportunity", "Opportunities", "OP"),
        new("crm.quote", "Quotes", "QT"),
        new("crm.salesOrder", "Sales orders", "SO"),
        new("crm.saleEnquiry", "Sale enquiries", "SE"),
        new("crm.salePreCost", "Sale pre-costs", "PC"),
        new("sales.dispatchAdvice", "Dispatch advices", "DA"),
        new("crm.customer", "Customers (created from leads)", "CU", 5, "Never"),
    ];

    public static SequenceDefault? Find(string key) => All.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
}

public interface IDocumentNumberService
{
    /// <summary>Issues the next number for the current tenant, e.g. QT-2026-00042. Never returns the same number twice.</summary>
    Task<string> NextAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// Per-tenant sequences. Each number is issued by a single UPDATE statement that increments and returns
/// the counter under a row lock, so concurrent requests get distinct, consecutive numbers.
/// A number taken by a request that later fails is not reused (sequences are near-gapless; statutory
/// gapless numbering for invoices is a Finance concern).
/// </summary>
public sealed class DocumentNumberService : IDocumentNumberService
{
    private static readonly Regex PrefixPattern = new("^[A-Za-z0-9]{1,20}$", RegexOptions.Compiled);

    private readonly DbContext _db;
    private readonly ITenantProvider _tenants;
    private readonly TimeProvider _clock;

    public DocumentNumberService(DbContext db, ITenantProvider tenants, TimeProvider clock)
    {
        _db = db;
        _tenants = tenants;
        _clock = clock;
    }

    private Guid TenantId => _tenants.GetTenantId() is Guid id && id != Guid.Empty
        ? id
        : throw new UnauthorizedException("Your session has no tenant. Please sign in again.");

    private int Year => _clock.GetUtcNow().Year;

    public async Task<string> NextAsync(string key, CancellationToken cancellationToken = default)
    {
        var definition = SequenceCatalog.Find(key) ?? throw new KeyNotFoundException($"Unknown document sequence '{key}'.");
        var tenantId = TenantId;
        await EnsureExistsAsync(tenantId, definition, cancellationToken);

        var issued = (await _db.Database
            .SqlQueryRaw<IssuedNumber>(IncrementSql(), tenantId, definition.Key, Year)
            .ToListAsync(cancellationToken)).Single();

        return Format(issued.Prefix, issued.ResetPolicy, Year, issued.Value, issued.Padding);
    }

    public async Task<IReadOnlyList<DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId;
        var stored = await _db.Set<DocumentSequence>().AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToDictionaryAsync(s => s.SequenceKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        return SequenceCatalog.All.Select(d => stored.TryGetValue(d.Key, out var s)
            ? ToDto(d, s.Prefix, s.Padding, s.ResetPolicy, NextFor(s))
            : ToDto(d, d.Prefix, d.Padding, d.ResetPolicy, 1)).ToList();
    }

    public async Task<DocumentSequenceDto> UpdateAsync(string key, UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default)
    {
        var definition = SequenceCatalog.Find(key) ?? throw new KeyNotFoundException($"Unknown document sequence '{key}'.");
        var tenantId = TenantId;
        await EnsureExistsAsync(tenantId, definition, cancellationToken);
        var sequence = await _db.Set<DocumentSequence>().SingleAsync(s => s.TenantId == tenantId && s.SequenceKey == definition.Key, cancellationToken);
        // NextAsync advances the counter with raw SQL, so a tracked copy can be stale. Validate against the database.
        await _db.Entry(sequence).ReloadAsync(cancellationToken);

        var errors = new List<string>();
        var prefix = dto.Prefix?.Trim() ?? "";
        if (!PrefixPattern.IsMatch(prefix)) errors.Add("Prefix must be 1–20 letters or digits.");
        if (dto.Padding is < 1 or > 10) errors.Add("Digits must be between 1 and 10.");
        if (dto.ResetPolicy is not ("Never" or "Yearly")) errors.Add("Reset must be Never or Yearly.");
        // Lowering the counter would hand out numbers that were already used.
        if (dto.NextNumber < NextFor(sequence)) errors.Add($"Next number can't be lower than {NextFor(sequence)}; that would reuse numbers already issued.");
        if (errors.Count > 0) throw new ValidationException("The numbering settings are not valid.", errors);

        sequence.Prefix = prefix.ToUpperInvariant();
        sequence.Padding = dto.Padding;
        sequence.ResetPolicy = dto.ResetPolicy;
        sequence.NextNumber = dto.NextNumber;
        // Changing to yearly starts counting in the current year rather than resetting on the next issue.
        if (dto.ResetPolicy == "Yearly") sequence.LastResetYear = Year;
        sequence.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(definition, sequence.Prefix, sequence.Padding, sequence.ResetPolicy, sequence.NextNumber);
    }

    public static string Format(string prefix, string resetPolicy, int year, long number, int padding) =>
        resetPolicy == "Yearly"
            ? $"{prefix}-{year}-{number.ToString().PadLeft(padding, '0')}"
            : $"{prefix}-{number.ToString().PadLeft(padding, '0')}";

    /// <summary>The number the next document will get, accounting for a pending yearly reset.</summary>
    private long NextFor(DocumentSequence s) => s.ResetPolicy == "Yearly" && s.LastResetYear != Year ? 1 : s.NextNumber;

    private DocumentSequenceDto ToDto(SequenceDefault d, string prefix, int padding, string policy, long next) => new()
    {
        Key = d.Key,
        Label = d.Label,
        Prefix = prefix,
        Padding = padding,
        ResetPolicy = policy,
        NextNumber = next,
        Preview = Format(prefix, policy, Year, next, padding),
    };

    private async Task EnsureExistsAsync(Guid tenantId, SequenceDefault definition, CancellationToken cancellationToken)
    {
        var set = _db.Set<DocumentSequence>();
        if (await set.AnyAsync(s => s.TenantId == tenantId && s.SequenceKey == definition.Key, cancellationToken)) return;

        var row = new DocumentSequence
        {
            TenantId = tenantId,
            SequenceKey = definition.Key,
            Prefix = definition.Prefix,
            Padding = definition.Padding,
            ResetPolicy = definition.ResetPolicy,
            NextNumber = 1,
            LastResetYear = definition.ResetPolicy == "Yearly" ? Year : null,
        };
        set.Add(row);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another request created it first (unique index on TenantId + SequenceKey). Use theirs.
            _db.Entry(row).State = EntityState.Detached;
        }
    }

    private string IncrementSql()
    {
        var entity = _db.Model.FindEntityType(typeof(DocumentSequence))!;
        var provider = _db.Database.ProviderName ?? "";
        var isSqlServer = provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
        var table = isSqlServer && entity.GetSchema() is string schema
            ? $"[{schema}].[{entity.GetTableName()}]"
            : $"\"{entity.GetTableName()}\"";

        // {0} tenant, {1} key, {2} current year. The yearly reset issues 1 and leaves 2 as the next number.
        const string set = """
            SET NextNumber = CASE WHEN ResetPolicy = 'Yearly' AND COALESCE(LastResetYear, 0) <> {2} THEN 2 ELSE NextNumber + 1 END,
                LastResetYear = CASE WHEN ResetPolicy = 'Yearly' THEN {2} ELSE LastResetYear END
            """;
        return isSqlServer
            ? $"UPDATE {table} WITH (ROWLOCK, UPDLOCK) {set} OUTPUT inserted.NextNumber - 1 AS Value, inserted.Prefix, inserted.Padding, inserted.ResetPolicy WHERE TenantId = {{0}} AND SequenceKey = {{1}}"
            : $"UPDATE {table} {set} WHERE TenantId = {{0}} AND SequenceKey = {{1}} RETURNING NextNumber - 1 AS Value, Prefix, Padding, ResetPolicy";
    }

    internal sealed class IssuedNumber
    {
        public long Value { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public int Padding { get; set; }
        public string ResetPolicy { get; set; } = string.Empty;
    }
}
