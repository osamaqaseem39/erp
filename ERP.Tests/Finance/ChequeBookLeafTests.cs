using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class ChequeBookLeafTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid BankAccount = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_Issue_RejectDoubleIssue_CancelLeaf()
    {
        await using var db = Db();
        var svc = new ChequeBookService(db);

        var book = await svc.CreateAsync(new CreateChequeBookDto
        {
            BankAccountId = BankAccount,
            BookNumber = "CB-1",
            StartLeafNumber = 1,
            EndLeafNumber = 3
        }, Tenant, User);

        Assert.Equal(3, book.Leaves.Count);
        Assert.All(book.Leaves, l => Assert.Equal("Blank", l.Status));

        var issued = await svc.IssueLeafAsync(book.Id, 1, new IssueChequeLeafDto { Notes = "pay vendor" }, Tenant, User);
        Assert.Equal("Issued", issued.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.IssueLeafAsync(book.Id, 1, new IssueChequeLeafDto(), Tenant, User));

        var cancelled = await svc.SetLeafStatusAsync(book.Id, 2, new SetChequeLeafStatusDto { Status = "Cancelled" }, Tenant, User);
        Assert.Equal("Cancelled", cancelled.Status);
    }
}
