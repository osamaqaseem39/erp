using ERP.Domain.Core;
using ERP.Infrastructure.Numbering;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

/// <summary>Runs the real increment SQL on a SQLite file database with one connection per request.</summary>
public sealed class DocumentNumberServiceTests : IDisposable
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly string _file = Path.Combine(Path.GetTempPath(), $"erp-seq-{Guid.NewGuid():N}.db");
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 9, 29, 10, 0, 0, TimeSpan.Zero));

    public DocumentNumberServiceTests()
    {
        using var db = NewDb();
        db.Database.EnsureCreated();
    }

    private SequenceDb NewDb() => new(new DbContextOptionsBuilder<SequenceDb>()
        .UseSqlite($"Data Source={_file};Default Timeout=30")
        .Options);

    private DocumentNumberService Service(SequenceDb db, Guid? tenant = null) => new(db, new FixedTenant(tenant ?? Tenant), _clock);

    [Fact]
    public async Task IssuesSequentialFormattedNumbers()
    {
        await using var db = NewDb();
        var numbers = Service(db);

        Assert.Equal("QT-2026-00001", await numbers.NextAsync("crm.quote"));
        Assert.Equal("QT-2026-00002", await numbers.NextAsync("crm.quote"));
        Assert.Equal("SO-2026-00001", await numbers.NextAsync("crm.salesOrder"));
        Assert.Equal("CU-00001", await numbers.NextAsync("crm.customer"));
    }

    [Fact]
    public async Task ConcurrentRequestsNeverGetTheSameNumber()
    {
        const int requests = 50;
        var results = await Task.WhenAll(Enumerable.Range(0, requests).Select(async _ =>
        {
            await using var db = NewDb();
            return await Service(db).NextAsync("crm.quote");
        }));

        Assert.Equal(requests, results.Distinct().Count());
        var expected = Enumerable.Range(1, requests).Select(n => $"QT-2026-{n:D5}").OrderBy(s => s);
        Assert.Equal(expected, results.OrderBy(s => s));
    }

    [Fact]
    public async Task EachTenantHasItsOwnSequence()
    {
        await using var db = NewDb();
        await Service(db).NextAsync("crm.quote");

        Assert.Equal("QT-2026-00001", await Service(db, Guid.NewGuid()).NextAsync("crm.quote"));
    }

    [Fact]
    public async Task YearlySequencesRestartInJanuary()
    {
        await using var db = NewDb();
        var numbers = Service(db);
        await numbers.NextAsync("crm.quote");
        await numbers.NextAsync("crm.quote");

        _clock.Now = new DateTimeOffset(2027, 1, 2, 9, 0, 0, TimeSpan.Zero);

        Assert.Equal("QT-2027-00001", await numbers.NextAsync("crm.quote"));
        Assert.Equal("QT-2027-00002", await numbers.NextAsync("crm.quote"));
    }

    [Fact]
    public async Task SettingsChangeTheFormat_ButCannotReuseIssuedNumbers()
    {
        await using var db = NewDb();
        var numbers = Service(db);
        await numbers.NextAsync("crm.quote"); // 1 issued, next is 2

        await Assert.ThrowsAsync<ValidationException>(() =>
            numbers.UpdateAsync("crm.quote", new UpdateDocumentSequenceDto { Prefix = "QUO", Padding = 4, ResetPolicy = "Never", NextNumber = 1 }));

        var updated = await numbers.UpdateAsync("crm.quote", new UpdateDocumentSequenceDto { Prefix = "quo", Padding = 4, ResetPolicy = "Never", NextNumber = 500 });
        Assert.Equal("QUO-0500", updated.Preview);
        Assert.Equal("QUO-0500", await numbers.NextAsync("crm.quote"));
    }

    [Theory]
    [InlineData("", 5, "Yearly")]
    [InlineData("Q T", 5, "Yearly")]
    [InlineData("QT", 0, "Yearly")]
    [InlineData("QT", 5, "Monthly")]
    public async Task InvalidSettingsAreRejected(string prefix, int padding, string policy)
    {
        await using var db = NewDb();
        await Assert.ThrowsAsync<ValidationException>(() =>
            Service(db).UpdateAsync("crm.quote", new UpdateDocumentSequenceDto { Prefix = prefix, Padding = padding, ResetPolicy = policy, NextNumber = 1 }));
    }

    [Fact]
    public async Task ListShowsEveryCatalogSequenceWithAPreview()
    {
        await using var db = NewDb();
        var list = await Service(db).ListAsync();

        Assert.Equal(SequenceCatalog.All.Count, list.Count);
        Assert.Equal("QT-2026-00001", list.Single(s => s.Key == "crm.quote").Preview);
    }

    [Fact]
    public async Task UnknownKeysAndMissingTenantsAreRejected()
    {
        await using var db = NewDb();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service(db).NextAsync("crm.nothing"));
        await Assert.ThrowsAsync<UnauthorizedException>(() => new DocumentNumberService(db, new FixedTenant(null), _clock).NextAsync("crm.quote"));
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var f in new[] { _file, _file + "-wal", _file + "-shm" })
            if (File.Exists(f)) File.Delete(f);
    }

    private sealed class SequenceDb(DbContextOptions<SequenceDb> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentSequence>(e =>
            {
                e.ToTable("DocumentSequences");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.TenantId, x.SequenceKey }).IsUnique();
            });
        }
    }

    private sealed class FixedTenant(Guid? id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private sealed class FakeClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
