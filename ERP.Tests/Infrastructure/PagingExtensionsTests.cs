using ERP.Domain;
using ERP.Infrastructure.Extensions;
using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Infrastructure;

/// <summary>Runs against SQLite so sort selectors are proven to translate to SQL, not just LINQ-to-objects.</summary>
public sealed class PagingExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestDbContext _db;

    private static readonly SortMap<Widget> Sort = new SortMap<Widget>("name")
        .Add("name", w => w.Name)
        .Add("rank", w => w.Rank)
        .Add("createdAt", w => w.CreatedAt);

    public PagingExtensionsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        // 12 rows; ranks repeat so the Id tie-breaker is exercised.
        for (var i = 1; i <= 12; i++)
            _db.Widgets.Add(new Widget { Name = $"W{i:00}", Rank = i % 3 });
        _db.SaveChanges();
    }

    [Fact]
    public async Task PagesAndCounts()
    {
        var page = await _db.Widgets.ToPagedResultAsync(new PagedRequest { PageNumber = 2, PageSize = 5 }, Sort);

        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(new[] { "W06", "W07", "W08", "W09", "W10" }, page.Items.Select(w => w.Name));
        Assert.True(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public async Task LastPageIsPartial()
    {
        var page = await _db.Widgets.ToPagedResultAsync(new PagedRequest { PageNumber = 3, PageSize = 5 }, Sort);

        Assert.Equal(new[] { "W11", "W12" }, page.Items.Select(w => w.Name));
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public async Task SortsDescendingByWhitelistedKeyCaseInsensitively()
    {
        var page = await _db.Widgets.ToPagedResultAsync(
            new PagedRequest { PageSize = 3, SortBy = "NAME", SortDescending = true }, Sort);

        Assert.Equal(new[] { "W12", "W11", "W10" }, page.Items.Select(w => w.Name));
    }

    [Fact]
    public async Task EqualSortValuesHaveAStableOrderAcrossPages()
    {
        var request = new PagedRequest { PageSize = 4, SortBy = "rank" };
        var all = new List<Guid>();
        for (var pageNumber = 1; pageNumber <= 3; pageNumber++)
        {
            request.PageNumber = pageNumber;
            var page = await _db.Widgets.ToPagedResultAsync(request, Sort);
            all.AddRange(page.Items.Select(w => w.Id));
        }

        Assert.Equal(12, all.Distinct().Count());
    }

    [Fact]
    public async Task UnknownSortKeyIsRejected()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _db.Widgets.ToPagedResultAsync(new PagedRequest { SortBy = "TenantId" }, Sort));

        Assert.Contains("TenantId", ex.Message);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-3, 5000, 1, PagedRequest.MaxPageSize)]
    public async Task OutOfRangePagingIsClamped(int pageNumber, int pageSize, int expectedPage, int expectedSize)
    {
        var page = await _db.Widgets.ToPagedResultAsync(new PagedRequest { PageNumber = pageNumber, PageSize = pageSize }, Sort);

        Assert.Equal(expectedPage, page.PageNumber);
        Assert.Equal(expectedSize, page.PageSize);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    public sealed class Widget : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Rank { get; set; }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Widget> Widgets => Set<Widget>();
    }
}
