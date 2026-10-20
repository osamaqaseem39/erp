using System;
using System.Threading.Tasks;
using ERP.Application.Inventory;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

var tenant = Guid.NewGuid();
var creator = Guid.NewGuid();
var from = Guid.NewGuid();
var to = Guid.NewGuid();
var item = Guid.NewGuid();
var db = new ERPDbContext(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
var svc = new StockTransferService(new StockTransferRepository(db), new StockService(new StockBalanceRepository(db), db: db));
try {
  var id = await svc.CreateAsync(new CreateStockTransferDto {
    FromWarehouseId = from, ToWarehouseId = to, TransferDate = DateTime.UtcNow.Date,
    Details = [ new CreateStockTransferDetailDto { ItemId = item, Quantity = 3 } ]
  }, tenant, creator);
  Console.WriteLine($""id={id}"");
  Console.WriteLine($""count={await db.StockTransfers.CountAsync()}"");
  var e = await db.StockTransfers.FirstOrDefaultAsync();
  Console.WriteLine($""entity tenant={e?.TenantId} status={e?.Status}"");
  var got = await svc.GetByIdAsync(id, tenant);
  Console.WriteLine($""got={(got==null?""null"":got.Status)} details={got?.Details?.Count}"");
} catch (Exception ex) { Console.WriteLine(ex); }
