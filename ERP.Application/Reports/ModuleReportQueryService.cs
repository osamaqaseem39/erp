using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Reports;

/// <summary>EF queries for catalog codes beyond the original sales BuiltIn set.</summary>
public interface IModuleReportQueryService
{
    Task<object> RunAsync(string code, Guid tenantId, DateTime? from, DateTime? to, Dictionary<string, object>? parameters);
}

public sealed class ModuleReportQueryService : IModuleReportQueryService
{
    private readonly ERPDbContext _db;

    public ModuleReportQueryService(ERPDbContext db) => _db = db;

    public Task<object> RunAsync(string code, Guid tenantId, DateTime? from, DateTime? to, Dictionary<string, object>? parameters)
    {
        if (tenantId == Guid.Empty)
            return Task.FromResult<object>(Empty(code));

        return code.ToUpperInvariant() switch
        {
            "PURCHASE_ORDER" => PurchaseOrderAsync(tenantId, from, to),
            "GOODS_RECEIPT_SUMMARY" => GoodsReceiptAsync(tenantId, from, to),
            "PURCHASE_DEBIT_NOTE" => PurchaseDebitNoteAsync(tenantId, from, to),
            "PURCHASE_RETURN_SUMMARY" => PurchaseReturnAsync(tenantId, from, to),
            "LANDED_COST_SUMMARY" => LandedCostAsync(tenantId, from, to),
            "STOCK_LEDGER" => StockLedgerAsync(tenantId, from, to, parameters),
            "STOCK_MOVEMENT_SUMMARY" => StockMovementAsync(tenantId, from, to),
            "STOCK_AVERAGE_RATE" => StockAverageRateAsync(tenantId),
            "STOCK_RESERVATION" => StockReservationAsync(tenantId),
            "CYCLE_COUNT_VARIANCE" => CycleCountVarianceAsync(tenantId),
            "LEAD_PIPELINE" => LeadPipelineAsync(tenantId, from, to),
            "OPPORTUNITY_PIPELINE" => OpportunityPipelineAsync(tenantId),
            "QUOTE_CONVERSION" => QuoteConversionAsync(tenantId, from, to),
            "CAMPAIGN_ACTIVITY" => CampaignActivityAsync(tenantId, from, to),
            "SALE_ENQUIRY_SUMMARY" => SaleEnquiryAsync(tenantId, from, to),
            "SALE_PRECOST_SUMMARY" => SalePreCostAsync(tenantId, from, to),
            "PRODUCTION_ORDER_STATUS" => ProductionOrderStatusAsync(tenantId),
            "BOM_USAGE" => BomUsageAsync(tenantId),
            "PRODUCTION_SCRAP" => ProductionScrapAsync(tenantId, from, to),
            "WIP_SUMMARY" => WipSummaryAsync(tenantId),
            "WORK_CENTER_LOAD" => WorkCenterLoadAsync(tenantId),
            "PROJECT_STATUS" => ProjectStatusAsync(tenantId),
            "PROJECT_TIMESHEET" => ProjectTimesheetAsync(tenantId, from, to),
            "PROJECT_BURN" => ProjectBurnAsync(tenantId),
            "MILESTONE_DUE" => MilestoneDueAsync(tenantId),
            "EMPLOYEE_ROSTER" => EmployeeRosterAsync(tenantId),
            "LEAVE_BALANCE" => LeaveBalanceAsync(tenantId),
            "LEAVE_UTILIZATION" => LeaveUtilizationAsync(tenantId, from, to),
            "ATTENDANCE_SUMMARY" => AttendanceSummaryAsync(tenantId, from, to),
            "PAYROLL_SUMMARY" => PayrollSummaryAsync(tenantId, from, to),
            "EMPLOYEE_ADVANCE" => EmployeeAdvanceAsync(tenantId),
            "POS_SESSION_SUMMARY" => PosSessionAsync(tenantId, from, to),
            "POS_TENDER_SUMMARY" => PosTenderAsync(tenantId, from, to),
            "POS_ITEM_SALES" => PosItemSalesAsync(tenantId, from, to),
            // Standard default pack (stubs until dedicated query methods land)
            "DISPATCH_ADVICE" or "SALE_RETURN" or "VENDOR_INFO" or "PURCHASE_REQUISITION"
                or "RFQ_SUMMARY" or "VENDOR_CONTRACT" or "STOCK_VALUATION" or "REORDER_ALERT"
                or "BATCH_EXPIRY" or "SERIAL_NUMBER_LIST" or "ITEM_KIT_LIST" or "PAYMENT_REGISTER"
                or "TAX_SUMMARY" or "FIXED_ASSET_REGISTER" or "DEPRECIATION_SCHEDULE"
                or "EXPENSE_CLAIM_SUMMARY" or "BANK_TRANSACTION" or "OPENING_BALANCE"
                or "QUOTE_LIST" or "ROUTING_LIST" or "BENEFIT_ENROLLMENT" or "TRAINING_SUMMARY"
                or "RECRUITMENT_PIPELINE" => Task.FromResult(Empty(code)),
            _ => Task.FromResult<object>(Empty(code))
        };
    }

    private static object Empty(string code) => new { ReportCode = code, TotalCount = 0, Lines = Array.Empty<object>() };

    private async Task<object> PurchaseOrderAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.PurchaseOrders.AsNoTracking().Where(o => o.TenantId == tenantId);
        if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(o => o.OrderDate <= to.Value.Date);
        var rows = await q.OrderByDescending(o => o.OrderDate).Take(5000).ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            o.PurchaseOrderNumber,
            o.VendorId,
            o.OrderDate,
            o.Status,
            o.TotalAmount,
            o.Currency
        });
        return new { ReportCode = "PURCHASE_ORDER", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> GoodsReceiptAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.GoodsReceiptNotes.AsNoTracking().Where(g => g.TenantId == tenantId);
        if (from.HasValue) q = q.Where(g => g.ReceivedDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(g => g.ReceivedDate <= to.Value.Date);
        var rows = await q.OrderByDescending(g => g.ReceivedDate).Take(5000).ToListAsync();
        var lines = rows.Select(g => new
        {
            g.Id,
            g.GRNNumber,
            g.POId,
            g.VendorId,
            g.ReceivedDate,
            g.Status,
            g.WarehouseId
        });
        return new { ReportCode = "GOODS_RECEIPT_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PurchaseDebitNoteAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.PurchaseDebitNotes.AsNoTracking().Where(d => d.TenantId == tenantId);
        if (from.HasValue) q = q.Where(d => d.DebitNoteDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(d => d.DebitNoteDate <= to.Value.Date);
        var rows = await q.OrderByDescending(d => d.DebitNoteDate).Take(5000).ToListAsync();
        var lines = rows.Select(d => new
        {
            d.Id,
            d.DebitNoteNumber,
            d.VendorId,
            d.DebitNoteDate,
            d.Status,
            d.TotalAmount
        });
        return new { ReportCode = "PURCHASE_DEBIT_NOTE", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PurchaseReturnAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.PurchaseReturns.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (from.HasValue) q = q.Where(r => r.ReturnDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.ReturnDate <= to.Value.Date);
        var rows = await q.OrderByDescending(r => r.ReturnDate).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.ReturnNumber,
            r.VendorId,
            r.ReturnDate,
            r.Status,
            r.TotalAmount
        });
        return new { ReportCode = "PURCHASE_RETURN_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> LandedCostAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.LandedCostVouchers.AsNoTracking().Where(v => v.TenantId == tenantId);
        if (from.HasValue) q = q.Where(v => v.VoucherDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(v => v.VoucherDate <= to.Value.Date);
        var rows = await q.OrderByDescending(v => v.VoucherDate).Take(5000).ToListAsync();
        var lines = rows.Select(v => new
        {
            v.Id,
            v.VoucherNumber,
            v.VoucherDate,
            v.Status,
            v.TotalAmount
        });
        return new { ReportCode = "LANDED_COST_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> StockLedgerAsync(Guid tenantId, DateTime? from, DateTime? to, Dictionary<string, object>? parameters)
    {
        var itemId = ParseGuid(parameters, "itemId");
        var q = _db.StockLedgers.AsNoTracking().Where(l => l.TenantId == tenantId);
        if (itemId.HasValue) q = q.Where(l => l.ItemId == itemId.Value);
        if (from.HasValue) q = q.Where(l => l.TransactionDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(l => l.TransactionDate <= to.Value.Date);
        var rows = await q.OrderByDescending(l => l.TransactionDate).Take(5000).ToListAsync();
        var lines = rows.Select(l => new
        {
            l.Id,
            l.ItemId,
            l.WarehouseId,
            l.TransactionDate,
            l.TransactionType,
            l.QuantityIn,
            l.QuantityOut,
            l.BalanceQty,
            l.UnitPrice,
            l.ReferenceDocType
        });
        return new { ReportCode = "STOCK_LEDGER", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> StockMovementAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var transfers = await _db.StockTransfers.AsNoTracking()
            .Where(t => t.TenantId == tenantId
                        && (!from.HasValue || t.TransferDate >= from.Value.Date)
                        && (!to.HasValue || t.TransferDate <= to.Value.Date))
            .OrderByDescending(t => t.TransferDate).Take(2500).ToListAsync();
        var adjustments = await _db.StockAdjustments.AsNoTracking()
            .Where(a => a.TenantId == tenantId
                        && (!from.HasValue || a.AdjustmentDate >= from.Value.Date)
                        && (!to.HasValue || a.AdjustmentDate <= to.Value.Date))
            .OrderByDescending(a => a.AdjustmentDate).Take(2500).ToListAsync();
        var lines = transfers.Select(t => (object)new
        {
            Type = "Transfer",
            t.Id,
            Number = t.Id.ToString(),
            Date = t.TransferDate,
            t.Status,
            FromWarehouseId = t.FromWarehouseId,
            ToWarehouseId = t.ToWarehouseId
        }).Concat(adjustments.Select(a => (object)new
        {
            Type = "Adjustment",
            a.Id,
            Number = a.AdjustmentNumber,
            Date = a.AdjustmentDate,
            a.Status,
            FromWarehouseId = a.WarehouseId,
            ToWarehouseId = (Guid?)null
        })).ToList();
        return new { ReportCode = "STOCK_MOVEMENT_SUMMARY", FromDate = from, ToDate = to, TotalCount = lines.Count, Lines = lines };
    }

    private async Task<object> StockAverageRateAsync(Guid tenantId)
    {
        var balances = await (
            from s in _db.StockBalances.AsNoTracking()
            join i in _db.Items.AsNoTracking() on s.ItemId equals i.Id into ij
            from i in ij.DefaultIfEmpty()
            where s.TenantId == tenantId && s.CurrentQuantity != 0
            select new
            {
                s.ItemId,
                ItemCode = i != null ? i.ItemCode : null,
                ItemName = i != null ? i.ItemName : null,
                s.WarehouseId,
                s.CurrentQuantity,
                AverageCost = i != null ? i.CostPrice : 0m
            }).Take(5000).ToListAsync();
        return new { ReportCode = "STOCK_AVERAGE_RATE", TotalCount = balances.Count, Lines = balances };
    }

    private async Task<object> StockReservationAsync(Guid tenantId)
    {
        var rows = await _db.StockReservations.AsNoTracking()
            .Include(r => r.Lines)
            .Where(r => r.TenantId == tenantId && r.Status == "Active")
            .OrderByDescending(r => r.CreatedAt).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.ReservationNumber,
            ReservationDate = r.CreatedAt,
            r.Status,
            r.SalesOrderId,
            WarehouseId = r.Lines.Select(l => (Guid?)l.WarehouseId).FirstOrDefault()
        });
        return new { ReportCode = "STOCK_RESERVATION", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> CycleCountVarianceAsync(Guid tenantId)
    {
        var counts = await _db.CycleCounts.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CountDate).Take(200).Select(c => c.Id).ToListAsync();
        var lines = await _db.CycleCountLines.AsNoTracking()
            .Where(l => counts.Contains(l.CycleCountId) && l.QuantityVariance != 0)
            .Take(5000)
            .Select(l => new
            {
                l.CycleCountId,
                l.ItemId,
                l.QuantityOnHand,
                l.QuantityCounted,
                l.QuantityVariance
            }).ToListAsync();
        return new { ReportCode = "CYCLE_COUNT_VARIANCE", TotalCount = lines.Count, Lines = lines };
    }

    private async Task<object> LeadPipelineAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Leads.AsNoTracking().Where(l => l.TenantId == tenantId);
        if (from.HasValue) q = q.Where(l => l.CreatedAt >= from.Value.Date);
        if (to.HasValue) q = q.Where(l => l.CreatedAt <= to.Value.Date.AddDays(1));
        var rows = await q.OrderByDescending(l => l.CreatedAt).Take(5000).ToListAsync();
        var lines = rows.Select(l => new
        {
            l.Id,
            LeadName = ((l.FirstName ?? "") + " " + (l.LastName ?? "")).Trim(),
            l.CompanyName,
            l.Status,
            l.Source,
            l.AssignedToUserId,
            l.CreatedAt
        });
        return new { ReportCode = "LEAD_PIPELINE", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> OpportunityPipelineAsync(Guid tenantId)
    {
        var rows = await _db.Opportunities.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status != "Lost" && o.Status != "Won")
            .OrderByDescending(o => o.ExpectedCloseDate).Take(5000).ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            o.OpportunityName,
            o.Stage,
            o.Status,
            o.Amount,
            o.Probability,
            o.ExpectedCloseDate,
            o.CustomerId
        });
        return new
        {
            ReportCode = "OPPORTUNITY_PIPELINE",
            TotalCount = rows.Count,
            TotalValue = Math.Round(rows.Sum(o => o.Amount), 2),
            Lines = lines
        };
    }

    private async Task<object> QuoteConversionAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Quotes.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from.HasValue) q = q.Where(x => x.QuoteDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(x => x.QuoteDate <= to.Value.Date);
        var quotes = await q.Take(5000).ToListAsync();
        var converted = quotes.Count(x => x.Status == "Accepted" || x.ConvertedDate != null);
        return new
        {
            ReportCode = "QUOTE_CONVERSION",
            FromDate = from,
            ToDate = to,
            QuoteCount = quotes.Count,
            ConvertedCount = converted,
            ConversionRate = quotes.Count == 0 ? 0 : Math.Round(100m * converted / quotes.Count, 2),
            Lines = quotes.Select(x => new
            {
                x.Id,
                x.QuoteNumber,
                x.QuoteDate,
                x.Status,
                x.TotalAmount,
                x.ConvertedDate,
                IsConverted = x.Status == "Accepted" || x.ConvertedDate != null,
                x.CustomerId
            })
        };
    }

    private async Task<object> CampaignActivityAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var campaigns = await _db.Campaigns.AsNoTracking().Where(c => c.TenantId == tenantId).Take(500).ToListAsync();
        var actQ = _db.Activities.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (from.HasValue) actQ = actQ.Where(a => a.StartDate >= from.Value.Date);
        if (to.HasValue) actQ = actQ.Where(a => a.StartDate <= to.Value.Date);
        var activities = await actQ.Take(5000).ToListAsync();
        return new
        {
            ReportCode = "CAMPAIGN_ACTIVITY",
            CampaignCount = campaigns.Count,
            ActivityCount = activities.Count,
            Lines = campaigns.Select(c => new
            {
                c.Id,
                c.CampaignName,
                c.Status,
                c.StartDate,
                c.EndDate
            }),
            Activities = activities.Select(a => new
            {
                a.Id,
                a.Subject,
                a.StartDate,
                a.Status,
                a.ActivityType
            })
        };
    }

    private async Task<object> SaleEnquiryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.SaleEnquiries.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (from.HasValue) q = q.Where(e => e.EnquiryDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(e => e.EnquiryDate <= to.Value.Date);
        var rows = await q.OrderByDescending(e => e.EnquiryDate).Take(5000).ToListAsync();
        var lines = rows.Select(e => new { e.Id, e.EnquiryNumber, e.EnquiryDate, e.Status, e.CustomerId });
        return new { ReportCode = "SALE_ENQUIRY_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> SalePreCostAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.SalePreCosts.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (from.HasValue) q = q.Where(e => e.PreCostDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(e => e.PreCostDate <= to.Value.Date);
        var rows = await q.OrderByDescending(e => e.PreCostDate).Take(5000).ToListAsync();
        var lines = rows.Select(e => new { e.Id, e.PreCostNumber, e.PreCostDate, e.Status, e.SaleEnquiryId });
        return new { ReportCode = "SALE_PRECOST_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> ProductionOrderStatusAsync(Guid tenantId)
    {
        var rows = await _db.ProductionOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId).OrderByDescending(o => o.PlannedStartDate).Take(5000).ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            o.ProductionOrderNumber,
            o.Status,
            o.ItemId,
            o.Quantity,
            o.PlannedStartDate,
            o.PlannedEndDate,
            o.SalesOrderId
        });
        return new { ReportCode = "PRODUCTION_ORDER_STATUS", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> BomUsageAsync(Guid tenantId)
    {
        var boms = await _db.BillOfMaterials.AsNoTracking().Where(b => b.TenantId == tenantId).Take(2000).ToListAsync();
        var bomIds = boms.Select(b => b.Id).ToList();
        var lineCounts = await _db.BillOfMaterialLines.AsNoTracking()
            .Where(l => bomIds.Contains(l.BillOfMaterialId))
            .GroupBy(l => l.BillOfMaterialId)
            .Select(g => new { BomId = g.Key, ComponentCount = g.Count() })
            .ToListAsync();
        var map = lineCounts.ToDictionary(x => x.BomId, x => x.ComponentCount);
        var lines = boms.Select(b => new
        {
            b.Id,
            b.BomNumber,
            ItemId = b.FinishedGoodItemId,
            b.Status,
            b.Version,
            ComponentCount = map.GetValueOrDefault(b.Id)
        });
        return new { ReportCode = "BOM_USAGE", TotalCount = boms.Count, Lines = lines };
    }

    private async Task<object> ProductionScrapAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.ProductionScraps.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (from.HasValue) q = q.Where(s => s.ScrapDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(s => s.ScrapDate <= to.Value.Date);
        var rows = await q.OrderByDescending(s => s.ScrapDate).Take(5000).ToListAsync();
        var lines = rows.Select(s => new
        {
            s.Id,
            s.ProductionOrderId,
            s.ItemId,
            s.Quantity,
            s.ScrapDate,
            s.ScrapReasonId,
            s.WarehouseId,
            s.Reason
        });
        return new { ReportCode = "PRODUCTION_SCRAP", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> WipSummaryAsync(Guid tenantId)
    {
        var rows = await _db.ProductionOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status != "Completed" && o.Status != "Cancelled")
            .Take(5000).ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            o.ProductionOrderNumber,
            o.Status,
            o.ItemId,
            o.Quantity,
            o.CompletedQuantity,
            OpenQty = o.Quantity - o.CompletedQuantity
        });
        return new { ReportCode = "WIP_SUMMARY", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> WorkCenterLoadAsync(Guid tenantId)
    {
        var centers = await _db.WorkCenters.AsNoTracking().Where(w => w.TenantId == tenantId).ToListAsync();
        var open = await _db.ProductionOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status != "Completed" && o.Status != "Cancelled")
            .ToListAsync();
        var lines = centers.Select(w => new
        {
            w.Id,
            w.WorkCenterCode,
            w.WorkCenterName,
            OpenOrders = open.Count(o => o.WorkCenterId == w.Id)
        });
        return new { ReportCode = "WORK_CENTER_LOAD", TotalCount = centers.Count, Lines = lines };
    }

    private async Task<object> ProjectStatusAsync(Guid tenantId)
    {
        var rows = await _db.Projects.AsNoTracking().Where(p => p.TenantId == tenantId).Take(2000).ToListAsync();
        var lines = rows.Select(p => new
        {
            p.Id,
            p.ProjectCode,
            p.ProjectName,
            p.Status,
            p.StartDate,
            p.EndDate,
            p.Budget,
            p.Progress
        });
        return new { ReportCode = "PROJECT_STATUS", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> ProjectTimesheetAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.ProjectTimesheets.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (from.HasValue) q = q.Where(t => t.PeriodStart >= from.Value.Date || t.CreatedAt >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.PeriodEnd <= to.Value.Date || t.CreatedAt <= to.Value.Date.AddDays(1));
        var headers = await q.Take(1000).ToListAsync();
        var ids = headers.Select(h => h.Id).ToList();
        var lines = await (
            from l in _db.ProjectTimesheetLines.AsNoTracking()
            join h in _db.ProjectTimesheets.AsNoTracking() on l.ProjectTimesheetId equals h.Id
            where ids.Contains(l.ProjectTimesheetId)
            select new
            {
                l.ProjectTimesheetId,
                h.ProjectId,
                l.ProjectTaskId,
                l.Hours,
                l.WorkDate,
                l.IsBillable
            }).Take(5000).ToListAsync();
        return new
        {
            ReportCode = "PROJECT_TIMESHEET",
            TotalCount = lines.Count,
            TotalHours = Math.Round(lines.Sum(l => l.Hours), 2),
            Lines = lines
        };
    }

    private async Task<object> ProjectBurnAsync(Guid tenantId)
    {
        var projects = await _db.Projects.AsNoTracking().Where(p => p.TenantId == tenantId).Take(500).ToListAsync();
        var expenses = await _db.ExpenseClaims.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.ProjectId != null)
            .GroupBy(e => e.ProjectId!.Value)
            .Select(g => new { ProjectId = g.Key, ExpenseTotal = g.Sum(x => x.TotalAmount) })
            .ToListAsync();
        var expenseMap = expenses.ToDictionary(x => x.ProjectId, x => x.ExpenseTotal);
        var lines = projects.Select(p => new
        {
            p.Id,
            p.ProjectCode,
            p.ProjectName,
            p.Budget,
            ExpenseTotal = expenseMap.GetValueOrDefault(p.Id),
            Remaining = p.Budget - expenseMap.GetValueOrDefault(p.Id)
        });
        return new { ReportCode = "PROJECT_BURN", TotalCount = projects.Count, Lines = lines };
    }

    private async Task<object> MilestoneDueAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.Date;
        var rows = await _db.ProjectMilestones.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.Status != "Completed")
            .OrderBy(m => m.TargetDate).Take(5000).ToListAsync();
        var lines = rows.Select(m => new
        {
            m.Id,
            m.ProjectId,
            m.MilestoneName,
            m.TargetDate,
            m.Status,
            IsOverdue = m.TargetDate.HasValue && m.TargetDate.Value.Date < today
        });
        return new { ReportCode = "MILESTONE_DUE", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> EmployeeRosterAsync(Guid tenantId)
    {
        var rows = await (
            from e in _db.Employees.AsNoTracking()
            join d in _db.Departments.AsNoTracking() on e.DepartmentId equals d.Id into dj
            from d in dj.DefaultIfEmpty()
            where e.TenantId == tenantId && e.Status == "Active"
            orderby e.LastName, e.FirstName
            select new
            {
                e.Id,
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                e.WorkEmail,
                Department = d != null ? d.Name : null,
                e.JobTitle,
                e.DateHired
            }).Take(5000).ToListAsync();
        return new { ReportCode = "EMPLOYEE_ROSTER", TotalCount = rows.Count, Lines = rows };
    }

    private async Task<object> LeaveBalanceAsync(Guid tenantId)
    {
        var rows = await (
            from b in _db.LeaveBalances.AsNoTracking()
            join t in _db.LeaveTypes.AsNoTracking() on b.LeaveTypeId equals t.Id into tj
            from t in tj.DefaultIfEmpty()
            join e in _db.Employees.AsNoTracking() on b.EmployeeId equals e.Id into ej
            from e in ej.DefaultIfEmpty()
            where b.TenantId == tenantId
            select new
            {
                b.EmployeeId,
                EmployeeName = e != null ? (e.FirstName + " " + e.LastName) : null,
                LeaveType = t != null ? t.Name : null,
                b.Accrued,
                b.Taken,
                Available = b.Accrued - b.Taken
            }).Take(5000).ToListAsync();
        return new { ReportCode = "LEAVE_BALANCE", TotalCount = rows.Count, Lines = rows };
    }

    private async Task<object> LeaveUtilizationAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.LeaveRequests.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (from.HasValue) q = q.Where(r => r.StartDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.EndDate <= to.Value.Date);
        var rows = await q.OrderByDescending(r => r.StartDate).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.EmployeeId,
            r.LeaveTypeId,
            r.StartDate,
            r.EndDate,
            r.TotalDays,
            r.DaysRequested,
            r.Status
        });
        return new { ReportCode = "LEAVE_UTILIZATION", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> AttendanceSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.TimeEntries.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (from.HasValue) q = q.Where(t => t.Date >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.Date <= to.Value.Date);
        var rows = await q.GroupBy(t => new { t.EmployeeId, WorkDate = t.Date })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.WorkDate,
                Hours = g.Sum(x => x.HoursWorked ?? 0m)
            }).Take(5000).ToListAsync();
        return new
        {
            ReportCode = "ATTENDANCE_SUMMARY",
            FromDate = from,
            ToDate = to,
            TotalCount = rows.Count,
            TotalHours = Math.Round(rows.Sum(r => r.Hours), 2),
            Lines = rows
        };
    }

    private async Task<object> PayrollSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Payrolls.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (from.HasValue) q = q.Where(p => p.PayPeriodEnd >= from.Value.Date);
        if (to.HasValue) q = q.Where(p => p.PayPeriodStart <= to.Value.Date);
        var rows = await q.OrderByDescending(p => p.PayPeriodEnd).Take(2000).ToListAsync();
        var lines = rows.Select(p => new
        {
            p.Id,
            p.PayPeriodStart,
            p.PayPeriodEnd,
            p.Status,
            p.TotalGross,
            p.TotalNet
        });
        return new { ReportCode = "PAYROLL_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> EmployeeAdvanceAsync(Guid tenantId)
    {
        var rows = await _db.EmployeeAdvances.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.Status != "Closed" && a.Status != "Cancelled")
            .OrderByDescending(a => a.AdvanceDate).Take(5000).ToListAsync();
        var lines = rows.Select(a => new
        {
            a.Id,
            a.EmployeeId,
            a.AdvanceDate,
            a.Amount,
            a.RecoveredAmount,
            Balance = a.Amount - a.RecoveredAmount,
            a.Status
        });
        return new { ReportCode = "EMPLOYEE_ADVANCE", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PosSessionAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.POSSessions.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (from.HasValue) q = q.Where(s => s.StartTime >= from.Value.Date);
        if (to.HasValue) q = q.Where(s => s.StartTime <= to.Value.Date.AddDays(1));
        var rows = await q.OrderByDescending(s => s.StartTime).Take(2000).ToListAsync();
        var lines = rows.Select(s => new
        {
            s.Id,
            s.SessionNumber,
            s.Status,
            s.StartTime,
            s.EndTime,
            s.OpeningBalance,
            s.ClosingBalance,
            s.ExpectedBalance
        });
        return new { ReportCode = "POS_SESSION_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PosTenderAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.POSTransactions.AsNoTracking().Where(t => t.TenantId == tenantId && t.Status != "Voided");
        if (from.HasValue) q = q.Where(t => t.TransactionDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.TransactionDate <= to.Value.Date);
        var rows = await q.GroupBy(t => t.PaymentMethod ?? "Unknown")
            .Select(g => new
            {
                Tender = g.Key,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(x => x.TotalAmount)
            }).ToListAsync();
        return new { ReportCode = "POS_TENDER_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = rows };
    }

    private async Task<object> PosItemSalesAsync(Guid tenantId, DateTime? fromDate, DateTime? toDate)
    {
        var fromD = fromDate?.Date;
        var toD = toDate?.Date;
        var q =
            from l in _db.POSTransactionLines.AsNoTracking()
            join t in _db.POSTransactions.AsNoTracking() on l.POSTransactionId equals t.Id
            where t.TenantId == tenantId && t.Status != "Voided"
                  && (fromD == null || t.TransactionDate >= fromD)
                  && (toD == null || t.TransactionDate <= toD)
            group l by new { l.ItemId, l.Description } into g
            select new
            {
                g.Key.ItemId,
                g.Key.Description,
                Quantity = g.Sum(x => x.Quantity),
                Amount = g.Sum(x => x.LineTotal)
            };
        var rows = await q.OrderByDescending(x => x.Amount).Take(5000).ToListAsync();
        return new { ReportCode = "POS_ITEM_SALES", FromDate = fromDate, ToDate = toDate, TotalCount = rows.Count, Lines = rows };
    }

    private async Task<object> DispatchAdviceAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.DispatchAdvices.AsNoTracking().Where(d => d.TenantId == tenantId);
        if (from.HasValue) q = q.Where(d => d.AdviceDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(d => d.AdviceDate <= to.Value.Date);
        var rows = await q.OrderByDescending(d => d.AdviceDate).Take(5000).ToListAsync();
        var lines = rows.Select(d => new
        {
            d.Id,
            d.AdviceNumber,
            d.AdviceDate,
            d.CustomerId,
            d.SalesOrderId,
            d.WarehouseId,
            d.Status,
            d.DispatchedAt
        });
        return new { ReportCode = "DISPATCH_ADVICE", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> SaleReturnAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.ReturnOrders.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (from.HasValue) q = q.Where(r => r.ReturnDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.ReturnDate <= to.Value.Date);
        var rows = await q.OrderByDescending(r => r.ReturnDate).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.ReturnNumber,
            r.ReturnDate,
            r.CustomerId,
            r.SalesInvoiceId,
            r.Status,
            r.TotalAmount,
            r.CreditNoteId
        });
        return new { ReportCode = "SALE_RETURN", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> VendorInfoAsync(Guid tenantId)
    {
        var rows = await _db.Vendors.AsNoTracking()
            .Where(v => v.TenantId == tenantId)
            .OrderBy(v => v.VendorName).Take(5000).ToListAsync();
        var lines = rows.Select(v => new
        {
            v.Id,
            v.VendorCode,
            v.VendorName,
            v.ContactPerson,
            v.PhoneNumber,
            v.Email,
            v.City,
            v.Country,
            v.TaxNumber,
            v.Ntn,
            v.Strn,
            v.CreditLimit,
            v.CreditDays,
            v.PaymentTerms,
            v.IsActive
        });
        return new { ReportCode = "VENDOR_INFO", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PurchaseRequisitionAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.PurchaseRequisitions.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (from.HasValue) q = q.Where(r => r.RequestDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.RequestDate <= to.Value.Date);
        var rows = await q.OrderByDescending(r => r.RequestDate).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.RequestDate,
            r.RequiredByDate,
            r.Status,
            r.RequestorId,
            r.DepartmentId,
            r.Remarks
        });
        return new { ReportCode = "PURCHASE_REQUISITION", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> RfqSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.RequestForQuotations.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (from.HasValue) q = q.Where(r => r.IssuedDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.IssuedDate <= to.Value.Date);
        var rows = await q.OrderByDescending(r => r.IssuedDate).Take(5000).ToListAsync();
        var lines = rows.Select(r => new
        {
            r.Id,
            r.RFQNumber,
            r.Title,
            r.IssuedDate,
            r.DueDate,
            r.Status,
            r.EstimatedValue,
            r.Priority,
            r.PRId
        });
        return new { ReportCode = "RFQ_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> VendorContractAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow.Date;
        var rows = await _db.VendorContracts.AsNoTracking()
            .Where(c => c.TenantId == tenantId
                        && c.ValidFrom <= today
                        && (c.ValidTo == null || c.ValidTo >= today))
            .OrderBy(c => c.VendorId).Take(5000).ToListAsync();
        var lines = rows.Select(c => new
        {
            c.Id,
            c.ContractNumber,
            c.VendorId,
            c.ItemId,
            c.UnitPrice,
            c.ValidFrom,
            c.ValidTo,
            c.PaymentTermId
        });
        return new { ReportCode = "VENDOR_CONTRACT", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> StockValuationAsync(Guid tenantId)
    {
        var rows = await (
            from s in _db.StockBalances.AsNoTracking()
            join i in _db.Items.AsNoTracking() on s.ItemId equals i.Id into ij
            from i in ij.DefaultIfEmpty()
            join w in _db.Warehouses.AsNoTracking() on s.WarehouseId equals w.Id into wj
            from w in wj.DefaultIfEmpty()
            where s.TenantId == tenantId && s.CurrentQuantity != 0
            select new
            {
                s.ItemId,
                ItemCode = i != null ? i.ItemCode : null,
                ItemName = i != null ? i.ItemName : null,
                s.WarehouseId,
                WarehouseName = w != null ? w.WarehouseName : null,
                s.CurrentQuantity,
                UnitCost = i != null ? i.CostPrice : 0m,
                Value = s.CurrentQuantity * (i != null ? i.CostPrice : 0m)
            }).Take(5000).ToListAsync();
        return new
        {
            ReportCode = "STOCK_VALUATION",
            TotalCount = rows.Count,
            TotalValue = Math.Round(rows.Sum(r => r.Value), 2),
            Lines = rows
        };
    }

    private async Task<object> ReorderAlertAsync(Guid tenantId)
    {
        var rules = await _db.ReorderRules.AsNoTracking().Where(r => r.TenantId == tenantId).ToListAsync();
        var balances = await _db.StockBalances.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync();
        var items = await _db.Items.AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .ToDictionaryAsync(i => i.Id, i => i);
        var lines = new List<object>();
        foreach (var rule in rules)
        {
            var qty = balances
                .Where(b => b.ItemId == rule.ItemId && b.WarehouseId == rule.WarehouseId)
                .Sum(b => b.CurrentQuantity);
            if (qty > rule.ReorderPoint) continue;
            items.TryGetValue(rule.ItemId, out var item);
            lines.Add(new
            {
                rule.ItemId,
                ItemCode = item?.ItemCode,
                ItemName = item?.ItemName,
                rule.WarehouseId,
                OnHand = qty,
                rule.ReorderPoint,
                rule.MinStockLevel,
                rule.ReorderQuantity,
                Shortfall = rule.ReorderPoint - qty
            });
        }
        return new { ReportCode = "REORDER_ALERT", TotalCount = lines.Count, Lines = lines.Take(5000) };
    }

    private async Task<object> BatchExpiryAsync(Guid tenantId)
    {
        var rows = await _db.BatchNumbers.AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.ExpiryDate != null)
            .OrderBy(b => b.ExpiryDate).Take(5000).ToListAsync();
        var today = DateTime.UtcNow.Date;
        var lines = rows.Select(b => new
        {
            b.Id,
            b.ItemId,
            b.BatchNumberValue,
            b.ManufacturingDate,
            b.ExpiryDate,
            b.Quantity,
            DaysToExpiry = b.ExpiryDate.HasValue ? (b.ExpiryDate.Value.Date - today).TotalDays : (double?)null,
            IsExpired = b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date < today
        });
        return new { ReportCode = "BATCH_EXPIRY", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> SerialNumberListAsync(Guid tenantId)
    {
        var rows = await _db.SerialNumbers.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SerialNumberValue).Take(5000).ToListAsync();
        var lines = rows.Select(s => new
        {
            s.Id,
            s.ItemId,
            s.SerialNumberValue,
            s.Status,
            s.Location,
            s.IsActive
        });
        return new { ReportCode = "SERIAL_NUMBER_LIST", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> ItemKitListAsync(Guid tenantId)
    {
        var kits = await _db.ItemKits.AsNoTracking().Where(k => k.TenantId == tenantId).Take(2000).ToListAsync();
        var kitIds = kits.Select(k => k.Id).ToList();
        var counts = await _db.ItemKitComponents.AsNoTracking()
            .Where(c => kitIds.Contains(c.ItemKitId))
            .GroupBy(c => c.ItemKitId)
            .Select(g => new { KitId = g.Key, ComponentCount = g.Count() })
            .ToListAsync();
        var map = counts.ToDictionary(x => x.KitId, x => x.ComponentCount);
        var lines = kits.Select(k => new
        {
            k.Id,
            k.Code,
            k.Name,
            k.KitItemId,
            ComponentCount = map.GetValueOrDefault(k.Id)
        });
        return new { ReportCode = "ITEM_KIT_LIST", TotalCount = kits.Count, Lines = lines };
    }

    private async Task<object> PaymentRegisterAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Payments.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (from.HasValue) q = q.Where(p => p.PaymentDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(p => p.PaymentDate <= to.Value.Date);
        var rows = await q.OrderByDescending(p => p.PaymentDate).Take(5000).ToListAsync();
        var lines = rows.Select(p => new
        {
            p.Id,
            p.PaymentNumber,
            p.PaymentDate,
            p.PaymentMethod,
            p.Amount,
            p.CurrencyCode,
            p.CustomerId,
            p.SupplierId,
            p.RelatedInvoiceId,
            p.RelatedBillId,
            p.Status,
            p.ReferenceNumber
        });
        return new { ReportCode = "PAYMENT_REGISTER", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> TaxSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.TaxTransactions.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (from.HasValue) q = q.Where(t => t.CreatedAt >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.CreatedAt <= to.Value.Date.AddDays(1));
        var rows = await q.OrderByDescending(t => t.CreatedAt).Take(5000).ToListAsync();
        var lines = rows.Select(t => new
        {
            t.TaxTransactionId,
            t.TaxCodeId,
            t.EntityReferenceTable,
            t.EntityReferenceId,
            t.TaxableAmount,
            t.TaxAmount,
            t.CreatedAt
        });
        return new
        {
            ReportCode = "TAX_SUMMARY",
            FromDate = from,
            ToDate = to,
            TotalCount = rows.Count,
            TotalTax = Math.Round(rows.Sum(t => t.TaxAmount), 2),
            Lines = lines
        };
    }

    private async Task<object> FixedAssetRegisterAsync(Guid tenantId)
    {
        var rows = await _db.FixedAssets.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.AssetCode).Take(5000).ToListAsync();
        var lines = rows.Select(a => new
        {
            a.Id,
            a.AssetCode,
            a.AssetName,
            a.AssetCategory,
            a.PurchaseDate,
            a.PurchaseCost,
            a.CurrentValue,
            a.Status,
            a.DisposalDate,
            a.CurrencyCode
        });
        return new
        {
            ReportCode = "FIXED_ASSET_REGISTER",
            TotalCount = rows.Count,
            TotalCost = Math.Round(rows.Sum(a => a.PurchaseCost), 2),
            TotalBookValue = Math.Round(rows.Sum(a => a.CurrentValue), 2),
            Lines = lines
        };
    }

    private async Task<object> DepreciationScheduleAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.DepreciationEntries.AsNoTracking().Where(d => d.TenantId == tenantId);
        if (from.HasValue) q = q.Where(d => d.PeriodEnd >= from.Value.Date);
        if (to.HasValue) q = q.Where(d => d.PeriodStart <= to.Value.Date);
        var rows = await q.OrderByDescending(d => d.PeriodEnd).Take(5000).ToListAsync();
        var lines = rows.Select(d => new
        {
            d.DepreciationId,
            d.AssetId,
            d.PeriodStart,
            d.PeriodEnd,
            d.DepreciationAmount,
            d.PostedJournalId
        });
        return new
        {
            ReportCode = "DEPRECIATION_SCHEDULE",
            FromDate = from,
            ToDate = to,
            TotalCount = rows.Count,
            TotalDepreciation = Math.Round(rows.Sum(d => d.DepreciationAmount), 2),
            Lines = lines
        };
    }

    private async Task<object> ExpenseClaimSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.ExpenseClaims.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (from.HasValue) q = q.Where(e => e.ClaimDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(e => e.ClaimDate <= to.Value.Date);
        var rows = await q.OrderByDescending(e => e.ClaimDate).Take(5000).ToListAsync();
        var lines = rows.Select(e => new
        {
            e.Id,
            e.ClaimNumber,
            e.ClaimDate,
            e.EmployeeId,
            e.ProjectId,
            e.Status,
            e.TotalAmount,
            e.CurrencyCode
        });
        return new { ReportCode = "EXPENSE_CLAIM_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> BankTransactionAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.BankTransactions.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (from.HasValue) q = q.Where(t => t.TransactionDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.TransactionDate <= to.Value.Date);
        var rows = await q.OrderByDescending(t => t.TransactionDate).Take(5000).ToListAsync();
        var lines = rows.Select(t => new
        {
            t.BankTxId,
            t.BankAccountId,
            t.TransactionDate,
            t.Amount,
            t.CurrencyCode,
            t.BankReference,
            t.Description,
            t.IsReconciled,
            t.MatchedPaymentId
        });
        return new { ReportCode = "BANK_TRANSACTION", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> OpeningBalanceAsync(Guid tenantId)
    {
        var headers = await _db.OpeningBalances.AsNoTracking()
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.AsOfDate).Take(200).ToListAsync();
        var ids = headers.Select(h => h.Id).ToList();
        var lines = await _db.OpeningBalanceLines.AsNoTracking()
            .Where(l => ids.Contains(l.OpeningBalanceId))
            .Take(5000)
            .Select(l => new
            {
                l.OpeningBalanceId,
                l.AccountId,
                l.Debit,
                l.Credit,
                l.CostCenterId,
                l.ProjectId,
                l.PartyType,
                l.PartyId,
                l.Narration
            }).ToListAsync();
        return new
        {
            ReportCode = "OPENING_BALANCE",
            HeaderCount = headers.Count,
            TotalCount = lines.Count,
            Headers = headers.Select(h => new { h.Id, h.FiscalYearId, h.AsOfDate, h.Status, h.ReferenceNo }),
            Lines = lines
        };
    }

    private async Task<object> QuoteListAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Quotes.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from.HasValue) q = q.Where(x => x.QuoteDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(x => x.QuoteDate <= to.Value.Date);
        var rows = await q.OrderByDescending(x => x.QuoteDate).Take(5000).ToListAsync();
        var lines = rows.Select(x => new
        {
            x.Id,
            x.QuoteNumber,
            x.QuoteDate,
            x.ValidUntil,
            x.Status,
            x.CustomerId,
            x.TotalAmount,
            x.Currency
        });
        return new { ReportCode = "QUOTE_LIST", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> RoutingListAsync(Guid tenantId)
    {
        var routings = await _db.Routings.AsNoTracking().Where(r => r.TenantId == tenantId).Take(2000).ToListAsync();
        var ids = routings.Select(r => r.Id).ToList();
        var stepCounts = await _db.RoutingSteps.AsNoTracking()
            .Where(s => ids.Contains(s.RoutingId))
            .GroupBy(s => s.RoutingId)
            .Select(g => new { RoutingId = g.Key, StepCount = g.Count() })
            .ToListAsync();
        var map = stepCounts.ToDictionary(x => x.RoutingId, x => x.StepCount);
        var lines = routings.Select(r => new
        {
            r.Id,
            r.RoutingNumber,
            r.Name,
            r.ItemId,
            r.Version,
            r.Status,
            StepCount = map.GetValueOrDefault(r.Id)
        });
        return new { ReportCode = "ROUTING_LIST", TotalCount = routings.Count, Lines = lines };
    }

    private async Task<object> BenefitEnrollmentAsync(Guid tenantId)
    {
        var rows = await _db.EmployeeBenefitEnrollments.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == "Active")
            .Take(5000).ToListAsync();
        var lines = rows.Select(e => new
        {
            e.Id,
            e.EmployeeId,
            e.BenefitId,
            e.EnrollmentDate,
            e.UnenrollmentDate,
            e.Status
        });
        return new { ReportCode = "BENEFIT_ENROLLMENT", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> TrainingSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        var q = _db.Trainings.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (from.HasValue) q = q.Where(t => t.StartDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(t => t.EndDate <= to.Value.Date);
        var rows = await q.OrderByDescending(t => t.StartDate).Take(2000).ToListAsync();
        var ids = rows.Select(t => t.Id).ToList();
        var enrollCounts = await _db.TrainingEnrollments.AsNoTracking()
            .Where(e => ids.Contains(e.TrainingId))
            .GroupBy(e => e.TrainingId)
            .Select(g => new { TrainingId = g.Key, Count = g.Count() })
            .ToListAsync();
        var map = enrollCounts.ToDictionary(x => x.TrainingId, x => x.Count);
        var lines = rows.Select(t => new
        {
            t.Id,
            t.Title,
            t.TrainingType,
            t.StartDate,
            t.EndDate,
            t.Status,
            t.Cost,
            EnrollmentCount = map.GetValueOrDefault(t.Id)
        });
        return new { ReportCode = "TRAINING_SUMMARY", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> RecruitmentPipelineAsync(Guid tenantId)
    {
        var rows = await _db.JobApplications.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.AppliedDate).Take(5000).ToListAsync();
        var lines = rows.Select(a => new
        {
            a.Id,
            a.JobOpeningId,
            a.CandidateId,
            a.AppliedDate,
            a.Status
        });
        var byStatus = rows.GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
        return new
        {
            ReportCode = "RECRUITMENT_PIPELINE",
            TotalCount = rows.Count,
            ByStatus = byStatus,
            Lines = lines
        };
    }

    private static Guid? ParseGuid(Dictionary<string, object>? parameters, string key)
    {
        if (parameters == null || !parameters.TryGetValue(key, out var raw) || raw == null) return null;
        if (raw is Guid g) return g;
        return Guid.TryParse(raw.ToString(), out var parsed) ? parsed : null;
    }
}
