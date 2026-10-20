using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IChequeBookService
{
    Task<IReadOnlyList<ChequeBookDto>> ListAsync(Guid tenantId);
    Task<ChequeBookDto?> GetAsync(Guid id, Guid tenantId);
    Task<ChequeBookDto> CreateAsync(CreateChequeBookDto dto, Guid tenantId, Guid userId);
    Task<ChequeLeafDto> IssueLeafAsync(Guid bookId, int leafNumber, IssueChequeLeafDto dto, Guid tenantId, Guid userId);
    Task<ChequeLeafDto> SetLeafStatusAsync(Guid bookId, int leafNumber, SetChequeLeafStatusDto dto, Guid tenantId, Guid userId);
}

public class ChequeBookService : IChequeBookService
{
    private static readonly HashSet<string> LeafStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Blank", "Issued", "Cleared", "Bounced", "Cancelled" };

    private readonly ERPDbContext _db;

    public ChequeBookService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<ChequeBookDto>> ListAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ChequeBookDto>();
        var rows = await _db.ChequeBooks.AsNoTracking()
            .Include(b => b.Leaves)
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderBy(b => b.BookNumber)
            .ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<ChequeBookDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.ChequeBooks.AsNoTracking()
            .Include(b => b.Leaves)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<ChequeBookDto> CreateAsync(CreateChequeBookDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.BankAccountId == Guid.Empty) throw new ValidationException("Bank account is required.");
        if (string.IsNullOrWhiteSpace(dto.BookNumber)) throw new ValidationException("Book number is required.");
        if (dto.EndLeafNumber < dto.StartLeafNumber) throw new ValidationException("Invalid leaf range.");

        var book = new ChequeBook
        {
            TenantId = tenantId,
            BankAccountId = dto.BankAccountId,
            BookNumber = dto.BookNumber.Trim(),
            StartLeafNumber = dto.StartLeafNumber,
            EndLeafNumber = dto.EndLeafNumber,
            Status = "Active",
            CreatedBy = userId.ToString()
        };
        for (var n = dto.StartLeafNumber; n <= dto.EndLeafNumber; n++)
        {
            book.Leaves.Add(new ChequeLeaf
            {
                TenantId = tenantId,
                LeafNumber = n,
                Status = "Blank",
                CreatedBy = userId.ToString()
            });
        }

        _db.ChequeBooks.Add(book);
        await _db.SaveChangesAsync();
        return Map(book);
    }

    public async Task<ChequeLeafDto> IssueLeafAsync(Guid bookId, int leafNumber, IssueChequeLeafDto dto, Guid tenantId, Guid userId)
    {
        var leaf = await GetLeafTrackedAsync(bookId, leafNumber, tenantId);
        if (!string.Equals(leaf.Status, "Blank", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only blank leaves can be issued.");
        leaf.Status = "Issued";
        leaf.IssuedAt = DateTime.UtcNow;
        leaf.JournalId = dto.JournalId;
        leaf.Notes = dto.Notes?.Trim();
        leaf.UpdatedBy = userId.ToString();
        leaf.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapLeaf(leaf);
    }

    public async Task<ChequeLeafDto> SetLeafStatusAsync(Guid bookId, int leafNumber, SetChequeLeafStatusDto dto, Guid tenantId, Guid userId)
    {
        if (!LeafStatuses.Contains(dto.Status)) throw new ValidationException("Invalid cheque leaf status.");
        var leaf = await GetLeafTrackedAsync(bookId, leafNumber, tenantId);
        if (string.Equals(leaf.Status, "Blank", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(dto.Status, "Issued", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(dto.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Blank leaf must be issued before other statuses.");
        leaf.Status = LeafStatuses.First(s => s.Equals(dto.Status, StringComparison.OrdinalIgnoreCase));
        if (string.Equals(leaf.Status, "Issued", StringComparison.OrdinalIgnoreCase) && leaf.IssuedAt == null)
            leaf.IssuedAt = DateTime.UtcNow;
        if (dto.Notes != null) leaf.Notes = dto.Notes.Trim();
        leaf.UpdatedBy = userId.ToString();
        leaf.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapLeaf(leaf);
    }

    private async Task<ChequeLeaf> GetLeafTrackedAsync(Guid bookId, int leafNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        var leaf = await _db.ChequeLeaves
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.ChequeBookId == bookId && l.LeafNumber == leafNumber);
        if (leaf == null) throw new NotFoundException("Cheque leaf not found.");
        return leaf;
    }

    private static ChequeBookDto Map(ChequeBook e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        BankAccountId = e.BankAccountId,
        BookNumber = e.BookNumber,
        StartLeafNumber = e.StartLeafNumber,
        EndLeafNumber = e.EndLeafNumber,
        Status = e.Status,
        Leaves = e.Leaves.OrderBy(l => l.LeafNumber).Select(MapLeaf).ToList()
    };

    private static ChequeLeafDto MapLeaf(ChequeLeaf l) => new()
    {
        Id = l.Id,
        LeafNumber = l.LeafNumber,
        Status = l.Status,
        IssuedAt = l.IssuedAt,
        JournalId = l.JournalId,
        Notes = l.Notes
    };
}
