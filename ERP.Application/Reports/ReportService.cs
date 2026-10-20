using System.Text.Json;
using ERP.Application.Core;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Reports;
using ERP.Application.Manufacturing;
using ERP.Domain.Reports;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Reports;

public class ReportService : IReportService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    private readonly IReportRepository _reportRepository;
    private readonly IReportExecutionRepository _reportExecutionRepository;
    private readonly IReportScheduleRepository _reportScheduleRepository;
    private readonly IAccountingService _accounting;
    private readonly IInvoiceService _invoices;
    private readonly IOperationalReportService? _operational;
    private readonly IModuleReportQueryService? _moduleQueries;
    private readonly IManufacturingReleaseService? _mfgRelease;
    private readonly ITenantEmailSender? _email;
    private readonly IPdfDocumentService? _pdf;
    private readonly ILogger<ReportService>? _logger;
    private readonly ERPDbContext? _db;

    public ReportService(
        IReportRepository reportRepository,
        IReportExecutionRepository reportExecutionRepository,
        IReportScheduleRepository reportScheduleRepository,
        IAccountingService accounting,
        IInvoiceService invoices,
        ERPDbContext? db = null,
        IOperationalReportService? operational = null,
        IModuleReportQueryService? moduleQueries = null,
        IManufacturingReleaseService? mfgRelease = null,
        ITenantEmailSender? email = null,
        IPdfDocumentService? pdf = null,
        ILogger<ReportService>? logger = null)
    {
        _reportRepository = reportRepository;
        _reportExecutionRepository = reportExecutionRepository;
        _reportScheduleRepository = reportScheduleRepository;
        _accounting = accounting;
        _invoices = invoices;
        _db = db;
        _operational = operational;
        _moduleQueries = moduleQueries;
        _mfgRelease = mfgRelease;
        _email = email;
        _pdf = pdf;
        _logger = logger;
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var report = await _reportRepository.GetByIdAsync(id, tenantId);
        return report != null ? MapToDto(report) : null;
    }

    public async Task<IEnumerable<ReportDto>> GetAllAsync(Guid tenantId, string? category = null, bool? isActive = null)
    {
        await EnsureBuiltInCatalogAsync(tenantId);
        var reports = await _reportRepository.GetAllAsync(tenantId);

        if (category != null)
            reports = reports.Where(r => r.Category == category);
        if (isActive.HasValue)
            reports = reports.Where(r => r.IsActive == isActive.Value);

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetByCategoryAsync(string category, Guid tenantId)
    {
        var reports = await _reportRepository.GetByCategoryAsync(category, tenantId);
        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetActiveAsync(Guid tenantId)
    {
        await EnsureBuiltInCatalogAsync(tenantId);
        var reports = await _reportRepository.GetActiveAsync(tenantId);
        return reports.Select(MapToDto);
    }

    public async Task<ReportDto> CreateAsync(CreateReportDto dto, Guid tenantId, Guid userId)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.ReportName : dto.Name,
            ReportName = string.IsNullOrWhiteSpace(dto.ReportName) ? dto.Name : dto.ReportName,
            ReportCode = dto.ReportCode,
            Description = dto.Description,
            ReportType = dto.ReportType,
            Category = dto.Category,
            Query = string.IsNullOrWhiteSpace(dto.ReportCode) ? (dto.Query ?? string.Empty) : dto.ReportCode.Trim(),
            Parameters = dto.Parameters ?? string.Empty,
            OutputFormat = dto.OutputFormat,
            IsActive = dto.IsActive,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _reportRepository.CreateAsync(report);
        var created = await _reportRepository.GetByIdAsync(createdId, tenantId);
        return MapToDto(created!);
    }

    public async Task<ReportDto> UpdateAsync(UpdateReportDto dto, Guid tenantId, Guid userId)
    {
        var report = await _reportRepository.GetByIdAsync(dto.Id, tenantId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{dto.Id}' not found.");

        report.Name = dto.Name;
        report.Description = dto.Description;
        report.Category = dto.Category;
        report.Query = dto.Query;
        report.Parameters = dto.Parameters?.ToString();
        report.OutputFormat = dto.OutputFormat;
        report.IsActive = dto.IsActive;
        report.UpdatedBy = userId.ToString();
        report.UpdatedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        return MapToDto(report);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _reportRepository.DeleteAsync(id, tenantId);
    }

    public Task<ReportExecutionDto> ExecuteAsync(Guid reportId, Dictionary<string, object>? parameters, Guid tenantId, Guid userId)
        => ExecuteAsync(reportId, parameters, tenantId, userId, scheduleId: null);

    private async Task<ReportExecutionDto> ExecuteAsync(
        Guid reportId,
        Dictionary<string, object>? parameters,
        Guid tenantId,
        Guid userId,
        Guid? scheduleId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, tenantId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{reportId}' not found.");

        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(),
            ReportId = reportId,
            ScheduleId = scheduleId,
            Parameters = parameters == null ? null : JsonSerializer.Serialize(parameters, JsonOpts),
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            TenantId = tenantId,
            ExecutedBy = userId == Guid.Empty ? "scheduler" : userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _reportExecutionRepository.CreateAsync(execution);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var payload = await RunBuiltInAsync(report.ReportCode, parameters, tenantId);
            var json = JsonSerializer.Serialize(payload, JsonOpts);
            execution.Status = "Completed";
            execution.CompletedAt = DateTime.UtcNow;
            execution.ResultData = json;
            execution.RecordCount = CountRecords(payload);
            sw.Stop();
            execution.ExecutionTimeMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            sw.Stop();
            execution.Status = "Failed";
            execution.ErrorMessage = ex.Message;
            execution.CompletedAt = DateTime.UtcNow;
            execution.ExecutionTimeMs = sw.ElapsedMilliseconds;
        }

        await _reportExecutionRepository.UpdateAsync(execution);
        var dto = MapToDto(execution);
        dto.ReportName = report.ReportName ?? report.Name;
        return dto;
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetExecutionsAsync(Guid reportId, Guid tenantId)
    {
        var executions = await _reportExecutionRepository.GetByReportIdAsync(reportId, tenantId);
        return executions.Select(MapToDto);
    }

    public async Task<ReportExecutionDto?> GetExecutionByIdAsync(Guid executionId, Guid tenantId)
    {
        var execution = await _reportExecutionRepository.GetByIdAsync(executionId, tenantId);
        return execution != null ? MapToDto(execution) : null;
    }

    public async Task<IEnumerable<ReportDto>> GetAvailableReportsAsync(Guid tenantId)
    {
        return await GetActiveAsync(tenantId);
    }

    public async Task<ReportExecutionDto> GenerateReportAsync(GenerateReportDto dto, Guid tenantId, Guid userId)
    {
        await EnsureBuiltInCatalogAsync(tenantId);
        var reportId = dto.ReportId;
        if (reportId == Guid.Empty && !string.IsNullOrWhiteSpace(dto.ReportType))
        {
            var byCode = await _reportRepository.GetByCodeAsync(dto.ReportType.Trim(), tenantId);
            if (byCode == null)
                throw new KeyNotFoundException($"Report code '{dto.ReportType}' not found.");
            reportId = byCode.Id;
        }

        var parameters = dto.Parameters ?? new Dictionary<string, object>();
        if (dto.StartDate.HasValue) parameters["fromDate"] = dto.StartDate.Value;
        if (dto.EndDate.HasValue) parameters["toDate"] = dto.EndDate.Value;
        return await ExecuteAsync(reportId, parameters, tenantId, userId);
    }

    public async Task<IEnumerable<ReportScheduleDto>> GetScheduledReportsAsync(Guid tenantId)
    {
        var schedules = await _reportScheduleRepository.GetByTenantIdAsync(tenantId);
        return schedules.Select(MapToDto);
    }

    public async Task<ReportScheduleDto> ScheduleReportAsync(ScheduleReportDto dto, Guid tenantId, Guid userId)
    {
        var report = await _reportRepository.GetByIdAsync(dto.ReportId, tenantId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID '{dto.ReportId}' not found.");

        var now = DateTime.UtcNow;
        var scheduleType = string.IsNullOrWhiteSpace(dto.ScheduleType) ? "Daily" : dto.ScheduleType.Trim();
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ReportId = dto.ReportId,
            ScheduleName = $"{report.ReportName ?? report.Name} ({scheduleType})",
            ScheduleType = scheduleType,
            CronExpression = dto.ScheduleExpression ?? string.Empty,
            NextRunTime = ReportScheduleCalculator.InitialNextRun(scheduleType, now),
            EmailRecipients = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            IsActive = true,
            TenantId = tenantId,
            CreatedBy = userId.ToString(),
            CreatedAt = now
        };

        var scheduleId = await _reportScheduleRepository.CreateAsync(schedule);
        var created = await _reportScheduleRepository.GetByIdAsync(scheduleId, tenantId);
        return MapToDto(created!);
    }

    public async Task<int> ExecuteDueSchedulesAsync(CancellationToken cancellationToken = default)
    {
        var due = (await _reportScheduleRepository.GetScheduledForExecutionAsync()).ToList();
        var ran = 0;
        foreach (var schedule in due)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (schedule.TenantId == Guid.Empty || schedule.ReportId == Guid.Empty)
                continue;

            var now = DateTime.UtcNow;
            ReportExecutionDto? execution = null;
            try
            {
                execution = await ExecuteAsync(schedule.ReportId, null, schedule.TenantId, Guid.Empty, schedule.Id);
            }
            catch
            {
                // ExecuteAsync already records Failed executions for built-in errors;
                // still advance the schedule so a bad report cannot block the queue forever.
            }

            if (execution != null
                && execution.Status == "Completed"
                && !string.IsNullOrWhiteSpace(schedule.EmailRecipients)
                && _email != null)
            {
                await DeliverScheduleEmailAsync(schedule, execution, cancellationToken);
            }

            schedule.LastRunTime = now;
            schedule.NextRunTime = ReportScheduleCalculator.ComputeNextRun(schedule.ScheduleType, now);
            schedule.UpdatedAt = now;
            schedule.UpdatedBy = "scheduler";
            await _reportScheduleRepository.UpdateAsync(schedule);
            ran++;
        }

        return ran;
    }

    private async Task DeliverScheduleEmailAsync(
        ReportSchedule schedule, ReportExecutionDto execution, CancellationToken cancellationToken)
    {
        var recipients = schedule.EmailRecipients!
            .Split([';', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (recipients.Length == 0) return;

        var csv = System.Text.Encoding.UTF8.GetBytes(JsonToFlatCsv(execution.ResultData ?? "{}"));
        var fileName = $"report_{schedule.ReportId:N}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var subject = $"Scheduled report: {schedule.ScheduleName}";
        var body = $"Report schedule '{schedule.ScheduleName}' completed at {DateTime.UtcNow:u} UTC. Status={execution.Status}; records={execution.RecordCount}.";

        foreach (var to in recipients)
        {
            try
            {
                var sent = await _email!.TrySendWithAttachmentAsync(
                    schedule.TenantId, to, subject, body, csv, fileName, "text/csv", cancellationToken);
                if (!sent)
                    _logger?.LogWarning("Schedule {ScheduleId}: SMTP not configured; email to {To} skipped.", schedule.Id, to);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Schedule {ScheduleId}: failed to email {To}.", schedule.Id, to);
            }
        }
    }

    public async Task<ReportScheduleDto?> GetScheduledReportByIdAsync(Guid id, Guid tenantId)
    {
        var schedule = await _reportScheduleRepository.GetByIdAsync(id, tenantId);
        return schedule != null ? MapToDto(schedule) : null;
    }

    public async Task<bool> CancelScheduledReportAsync(Guid id, Guid tenantId)
    {
        var schedule = await _reportScheduleRepository.GetByIdAsync(id, tenantId);
        if (schedule == null) return false;

        schedule.IsActive = false;
        await _reportScheduleRepository.UpdateAsync(schedule);
        return true;
    }

    public async Task<IEnumerable<ReportExecutionDto>> GetReportHistoryAsync(Guid tenantId, string? reportType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var executions = await _reportExecutionRepository.GetByTenantIdAsync(tenantId);
        
        if (reportType != null)
            executions = executions.Where(e => e.Report?.Category == reportType);
        if (fromDate.HasValue)
            executions = executions.Where(e => e.StartedAt >= fromDate.Value);
        if (toDate.HasValue)
            executions = executions.Where(e => e.StartedAt <= toDate.Value);

        return executions.Select(MapToDto);
    }

    public async Task<ReportExportDto> ExportReportAsync(Guid reportId, Guid tenantId, string format = "json")
    {
        var execution = await ExecuteAsync(reportId, null, tenantId, Guid.Empty);
        var json = execution.ResultData ?? "{}";
        var fmt = (format ?? "json").Trim().ToLowerInvariant();
        var report = await _reportRepository.GetByIdAsync(reportId, tenantId);
        var title = report?.ReportName ?? report?.Name ?? reportId.ToString("N");

        byte[] content;
        string contentType;
        string extension;
        if (fmt == "csv")
        {
            content = System.Text.Encoding.UTF8.GetBytes(JsonToFlatCsv(json));
            contentType = "text/csv";
            extension = "csv";
        }
        else if (fmt is "excel" or "xlsx" or "xls")
        {
            // SpreadsheetML that Excel opens (no third-party package).
            content = System.Text.Encoding.UTF8.GetBytes(JsonToSpreadsheetMl(json, title));
            contentType = "application/vnd.ms-excel";
            extension = "xls";
        }
        else if (fmt == "pdf")
        {
            if (_pdf == null)
                throw new NotSupportedException("PDF export is not configured.");
            var lines = JsonToPdfLines(json, title);
            content = _pdf.BuildSimpleDocument(title, lines);
            contentType = "application/pdf";
            extension = "pdf";
        }
        else
        {
            content = System.Text.Encoding.UTF8.GetBytes(json);
            contentType = "application/json";
            extension = "json";
        }

        if (execution.Id != Guid.Empty && !string.IsNullOrEmpty(execution.ResultData))
        {
            // Best-effort: stamp output hint on execution via repository update if available.
        }

        return new ReportExportDto
        {
            Content = content,
            ContentType = contentType,
            FileName = $"report_{reportId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}"
        };
    }

    private static string JsonToSpreadsheetMl(string json, string title)
    {
        var csv = JsonToFlatCsv(json);
        var rows = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("""<?xml version="1.0"?>""");
        sb.AppendLine("""<?mso-application progid="Excel.Sheet"?>""");
        sb.AppendLine("""<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet">""");
        sb.AppendLine($"<Worksheet ss:Name=\"{System.Security.SecurityElement.Escape(title) ?? "Report"}\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"><Table>");
        foreach (var row in rows)
        {
            sb.Append("<Row>");
            foreach (var cell in ParseCsvRow(row))
                sb.Append($"<Cell><Data ss:Type=\"String\">{System.Security.SecurityElement.Escape(cell)}</Data></Cell>");
            sb.AppendLine("</Row>");
        }
        sb.AppendLine("</Table></Worksheet></Workbook>");
        return sb.ToString();
    }

    private static IEnumerable<string> ParseCsvRow(string row)
    {
        var list = new List<string>();
        var cur = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < row.Length; i++)
        {
            var c = row[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < row.Length && row[i + 1] == '"') { cur.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                list.Add(cur.ToString());
                cur.Clear();
            }
            else cur.Append(c);
        }
        list.Add(cur.ToString());
        return list;
    }

    private static IReadOnlyList<string> JsonToPdfLines(string json, string title)
    {
        var lines = new List<string> { title, $"Generated: {DateTime.UtcNow:u} UTC", "" };
        var csv = JsonToFlatCsv(json);
        foreach (var row in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(200))
            lines.Add(row.Length > 120 ? row[..120] + "…" : row);
        if (csv.Split('\n').Length > 200)
            lines.Add("… truncated …");
        return lines;
    }

    private static string JsonToFlatCsv(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("Lines", out var lines)
                && lines.ValueKind == JsonValueKind.Array
                && lines.GetArrayLength() > 0)
            {
                var rows = lines.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Object).ToList();
                var headers = new List<string>();
                foreach (var row in rows)
                {
                    foreach (var prop in row.EnumerateObject())
                    {
                        if (!headers.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                            headers.Add(prop.Name);
                    }
                }
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(',', headers.Select(CsvEscape)));
                foreach (var row in rows)
                {
                    var cells = headers.Select(h =>
                        row.TryGetProperty(h, out var v) ? CsvEscape(JsonValueToString(v)) : "");
                    sb.AppendLine(string.Join(',', cells));
                }
                return sb.ToString();
            }

            // Fallback: single-row property dump
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var props = doc.RootElement.EnumerateObject().ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(',', props.Select(p => CsvEscape(p.Name))));
                sb.AppendLine(string.Join(',', props.Select(p => CsvEscape(JsonValueToString(p.Value)))));
                return sb.ToString();
            }
        }
        catch
        {
            // fall through
        }

        return CsvEscape(json);
    }

    private static string JsonValueToString(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.String => v.GetString() ?? "",
        JsonValueKind.Number => v.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => "",
        _ => v.GetRawText()
    };

    private static string CsvEscape(string? value)
    {
        value ??= "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    public async Task<object> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
        => await _accounting.GetTrialBalanceAsync(tenantId, fromDate, toDate, currencyCode);

    public async Task<object> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
        => await _accounting.GetProfitAndLossAsync(tenantId, fromDate, toDate, currencyCode);

    public async Task<object> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null)
        => await _accounting.GetBalanceSheetAsync(tenantId, asOfDate, currencyCode);

    public async Task<object> GetCashFlowAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
        => await _accounting.GetCashFlowAsync(tenantId, fromDate, toDate, currencyCode);

    public async Task<object> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
        => await _invoices.GetAgedReceivablesAsync(tenantId, asOfDate, agingBuckets);

    public async Task<object> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
        => await _invoices.GetAgedPayablesAsync(tenantId, asOfDate, agingBuckets);

    public async Task<object> GetGeneralLedgerAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate)
        => await _accounting.GetGeneralLedgerBalanceAsync(accountId, tenantId, fromDate, toDate);

    public async Task<object> GetBudgetVsActualAsync(Guid tenantId, DateTime fromDate, DateTime toDate, Guid? budgetId = null)
        => await _accounting.GetBudgetVsActualAsync(tenantId, fromDate, toDate, budgetId);

    private async Task EnsureBuiltInCatalogAsync(Guid tenantId)
    {
        if (_db == null || tenantId == Guid.Empty) return;

        foreach (var def in ReportCatalog.All)
        {
            var exists = await _db.Reports.AnyAsync(r =>
                r.TenantId == tenantId && r.ReportCode == def.Code);
            if (exists) continue;
            _db.Reports.Add(new Report
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReportCode = def.Code,
                Name = def.Name,
                ReportName = def.Name,
                Description = def.Description,
                Category = def.Category,
                ReportType = def.ReportType,
                Query = def.Code, // internal query key only — never expose to clients
                Parameters = "{\"fromDate\":\"date\",\"toDate\":\"date\"}",
                OutputFormat = "json",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task<object> RunBuiltInAsync(string? code, Dictionary<string, object>? parameters, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return new { Lines = Array.Empty<object>(), TotalCount = 0 };

        var key = (code ?? string.Empty).Trim().ToUpperInvariant();
        var (from, to) = ParseDateRange(parameters);
        var fromD = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var toD = to ?? DateTime.UtcNow.Date;

        // Prefer operational service where available to avoid duplicated queries.
        if (_operational != null)
        {
            object? ops = key switch
            {
                "PENDING_SALE_ORDER" => await _operational.GetPendingSalesOrdersAsync(tenantId, to),
                "SALE_INVOICE_SUMMARY" => await _operational.GetSalesInvoiceSummaryAsync(tenantId, fromD, toD),
                "SALE_SUMMARY_ITEM" => await _operational.GetSalesSummaryByItemAsync(tenantId, fromD, toD),
                "SALE_SUMMARY_CUSTOMER_ITEM" => await _operational.GetSalesSummaryCustomerItemAsync(tenantId, fromD, toD),
                "SALE_SUMMARY_ITEM_CUSTOMER" => await _operational.GetSalesSummaryItemCustomerAsync(tenantId, fromD, toD),
                "SALE_ITEM_RATE_DETAIL" => await _operational.GetSalesItemRateDetailAsync(tenantId, fromD, toD),
                "CUSTOMER_INFO" => await _operational.GetCustomerInformationAsync(tenantId),
                "CREDIT_NOTE" => await _operational.GetCreditNoteReportAsync(tenantId, fromD, toD),
                "PURCHASE_INVOICE_SUMMARY" => await _operational.GetPurchaseInvoiceSummaryAsync(tenantId, fromD, toD),
                "STOCK_ON_HAND" => await _operational.GetStockBalanceReportAsync(tenantId),
                "SALE_ORDER_SCHEDULE" => await _operational.GetSalesOrderScheduleAsync(tenantId, from, to),
                _ => null
            };
            if (ops != null) return ops;
        }

        if (key == "SALE_PENDING_FOR_PRODUCTION" && _mfgRelease != null)
        {
            var pending = await _mfgRelease.GetPendingSalesOrdersAsync(tenantId);
            return new { ReportCode = key, TotalCount = pending.Count, Lines = pending };
        }

        object? finance = key switch
        {
            "TRIAL_BALANCE" => await _accounting.GetTrialBalanceAsync(tenantId, fromD, toD),
            "RUNNING_TRIAL_BALANCE" => await _accounting.GetRunningTrialBalanceAsync(tenantId, fromD, toD),
            "PROFIT_LOSS" => await _accounting.GetProfitAndLossAsync(tenantId, fromD, toD),
            "BALANCE_SHEET" => await _accounting.GetBalanceSheetAsync(tenantId, toD),
            "GENERAL_LEDGER" => await RunGeneralLedgerAsync(tenantId, fromD, toD, parameters),
            "VOUCHER_LIST" => await _accounting.GetVoucherReportAsync(tenantId, from, to),
            "CHART_OF_ACCOUNTS" => await _accounting.GetChartOfAccountReportAsync(tenantId),
            "DAILY_FINANCIAL" => await _accounting.GetDailyFinancialAsync(tenantId, toD),
            "CASH_BANK_VOUCHERS" => await _accounting.GetCashBankVoucherSummaryAsync(tenantId, fromD, toD),
            "CASH_BANK_LEDGER" => await _accounting.GetCashBankLedgerSummaryAsync(tenantId, fromD, toD),
            "VOUCHER_TYPE_MONTHLY" => await _accounting.GetVoucherTypeMonthlySummaryAsync(tenantId, fromD, toD),
            "PARTNER_BALANCES" => await _invoices.GetPartnerBalanceSummaryAsync(tenantId, toD),
            "AGED_RECEIVABLES" or "SALE_AGING" => await _invoices.GetAgedReceivablesAsync(tenantId, toD),
            "AGED_PAYABLES" => await _invoices.GetAgedPayablesAsync(tenantId, toD),
            "CASH_FLOW" => await _accounting.GetCashFlowAsync(tenantId, fromD, toD),
            "BUDGET_VS_ACTUAL" => await _accounting.GetBudgetVsActualAsync(tenantId, fromD, toD),
            "CHEQUE_REGISTER" => await _accounting.GetChequeRegisterAsync(tenantId, fromD, toD),
            _ => null
        };
        if (finance != null) return finance;

        return key switch
        {
            "SALE_ORDER" => await SaleOrderListAsync(tenantId, from, to),
            "SALE_INVOICE" => await SaleInvoiceListAsync(tenantId, from, to),
            "CREDIT_NOTE" => await CreditNoteListAsync(tenantId, from, to),
            "CUSTOMER_INFO" => await CustomerInfoAsync(tenantId),
            "PENDING_SALE_ORDER" => await PendingSaleOrderAsync(tenantId),
            "SALE_INVOICE_SUMMARY" => await SaleInvoiceSummaryAsync(tenantId, from, to),
            "SALE_SUMMARY_ITEM" => await SaleSummaryItemAsync(tenantId, from, to),
            "SALE_SUMMARY_CUSTOMER_ITEM" => await SaleSummaryCustomerItemAsync(tenantId, from, to),
            "STOCK_ON_HAND" => await StockOnHandAsync(tenantId),
            "PURCHASE_ORDER_SUMMARY" => await PurchaseOrderSummaryAsync(tenantId, from, to),
            _ when _moduleQueries != null => await _moduleQueries.RunAsync(key, tenantId, from, to, parameters),
            _ => throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(code)
                    ? "Report has no ReportCode; cannot execute."
                    : $"Unknown report code '{code}'.")
        };
    }

    private async Task<object> RunGeneralLedgerAsync(
        Guid tenantId, DateTime fromD, DateTime toD, Dictionary<string, object>? parameters)
    {
        Guid? accountId = null;
        if (parameters != null && parameters.TryGetValue("accountId", out var raw) && raw != null)
        {
            if (raw is Guid g) accountId = g;
            else if (Guid.TryParse(raw.ToString(), out var parsed)) accountId = parsed;
        }
        if (accountId == null || accountId == Guid.Empty)
            return new { ReportCode = "GENERAL_LEDGER", Message = "accountId parameter required", Lines = Array.Empty<object>() };
        return await _accounting.GetGeneralLedgerBalanceAsync(accountId.Value, tenantId, fromD, toD);
    }

    private async Task<object> SaleOrderListAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>(), TotalCount = 0 };
        var q = _db.SalesOrders.AsNoTracking().Where(o => o.TenantId == tenantId);
        if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(o => o.OrderDate <= to.Value.Date);
        var rows = await q.OrderByDescending(o => o.OrderDate).Take(5_000).ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            OrderNumber = string.IsNullOrWhiteSpace(o.SalesOrderNumber) ? o.OrderNumber : o.SalesOrderNumber,
            o.CustomerId,
            o.OrderDate,
            o.RequiredDate,
            o.Status,
            o.SubTotal,
            o.TaxAmount,
            o.TotalAmount,
            o.Currency
        });
        return new { ReportCode = "SALE_ORDER", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> SaleInvoiceListAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>(), TotalCount = 0 };
        var q = _db.SalesInvoices.AsNoTracking().Where(i => i.TenantId == tenantId);
        if (from.HasValue) q = q.Where(i => i.InvoiceDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(i => i.InvoiceDate <= to.Value.Date);
        var rows = await q.OrderByDescending(i => i.InvoiceDate).Take(5_000).ToListAsync();
        var lines = rows.Select(i => new
        {
            i.Id,
            i.InvoiceNumber,
            i.CustomerId,
            i.InvoiceDate,
            i.DueDate,
            i.Status,
            i.SubTotal,
            i.TaxAmount,
            i.TotalAmount,
            i.PaidAmount,
            i.BalanceAmount,
            i.SalesOrderId
        });
        return new { ReportCode = "SALE_INVOICE", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> CreditNoteListAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>(), TotalCount = 0 };
        var q = _db.SalesCreditNotes.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (from.HasValue) q = q.Where(c => c.CreditNoteDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(c => c.CreditNoteDate <= to.Value.Date);
        var rows = await q.OrderByDescending(c => c.CreditNoteDate).Take(5_000).ToListAsync();
        var lines = rows.Select(c => new
        {
            c.Id,
            c.CreditNoteNumber,
            c.CustomerId,
            SalesInvoiceId = c.OriginalSalesInvoiceId,
            c.CreditNoteDate,
            c.Status,
            c.TotalAmount,
            c.Reason
        });
        return new { ReportCode = "CREDIT_NOTE", FromDate = from, ToDate = to, TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> CustomerInfoAsync(Guid tenantId)
    {
        if (_db == null) return new { Lines = Array.Empty<object>(), TotalCount = 0 };
        var rows = await _db.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.CustomerName)
            .Take(5_000)
            .ToListAsync();
        var lines = rows.Select(c => new
        {
            c.Id,
            c.CustomerCode,
            c.CustomerName,
            c.CompanyName,
            c.Email,
            c.Phone,
            TaxNumber = c.Ntn ?? c.TaxId,
            c.CreditLimit,
            PaymentTermsDays = c.CreditDays,
            c.TaxType,
            c.Ntn,
            c.Strn,
            c.Cnic,
            c.SalesmanId,
            c.IsActive
        });
        return new { ReportCode = "CUSTOMER_INFO", TotalCount = rows.Count, Lines = lines };
    }

    private async Task<object> PendingSaleOrderAsync(Guid tenantId)
    {
        if (_db == null) return new { Lines = Array.Empty<object>(), TotalCount = 0 };
        var closed = new[] { "Cancelled", "Delivered", "Invoiced", "Closed" };
        var rows = await _db.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId
                        && !closed.Contains(o.Status)
                        && o.InvoicedDate == null)
            .OrderBy(o => o.RequiredDate ?? o.OrderDate)
            .Take(5_000)
            .ToListAsync();
        var lines = rows.Select(o => new
        {
            o.Id,
            OrderNumber = string.IsNullOrWhiteSpace(o.SalesOrderNumber) ? o.OrderNumber : o.SalesOrderNumber,
            o.CustomerId,
            o.OrderDate,
            o.RequiredDate,
            o.Status,
            o.TotalAmount,
            OpenValue = o.TotalAmount
        });
        return new
        {
            ReportCode = "PENDING_SALE_ORDER",
            TotalCount = rows.Count,
            TotalOpenValue = Math.Round(rows.Sum(o => o.TotalAmount), 2),
            Lines = lines
        };
    }

    private async Task<object> SaleInvoiceSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>() };
        var q = _db.SalesInvoices.AsNoTracking().Where(i => i.TenantId == tenantId);
        if (from.HasValue) q = q.Where(i => i.InvoiceDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(i => i.InvoiceDate <= to.Value.Date);
        var rows = await q.ToListAsync();
        var byStatus = rows.GroupBy(i => i.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = Math.Round(g.Sum(x => x.TotalAmount), 2),
                BalanceAmount = Math.Round(g.Sum(x => x.BalanceAmount), 2)
            })
            .OrderBy(x => x.Status)
            .ToList();
        return new
        {
            ReportCode = "SALE_INVOICE_SUMMARY",
            FromDate = from,
            ToDate = to,
            InvoiceCount = rows.Count,
            TotalAmount = Math.Round(rows.Sum(i => i.TotalAmount), 2),
            TotalPaid = Math.Round(rows.Sum(i => i.PaidAmount), 2),
            TotalBalance = Math.Round(rows.Sum(i => i.BalanceAmount), 2),
            ByStatus = byStatus
        };
    }

    private async Task<object> SaleSummaryItemAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>() };
        var invQ = _db.SalesInvoices.AsNoTracking().Where(i => i.TenantId == tenantId);
        if (from.HasValue) invQ = invQ.Where(i => i.InvoiceDate >= from.Value.Date);
        if (to.HasValue) invQ = invQ.Where(i => i.InvoiceDate <= to.Value.Date);
        var invoiceIds = await invQ.Select(i => i.Id).ToListAsync();
        var lines = await _db.SalesInvoiceLines.AsNoTracking()
            .Where(l => invoiceIds.Contains(l.SalesInvoiceId))
            .ToListAsync();
        var itemIds = lines.Select(l => l.ItemId).Distinct().ToList();
        var items = await _db.Items.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync();
        var summary = lines.GroupBy(l => l.ItemId)
            .Select(g =>
            {
                var item = items.FirstOrDefault(i => i.Id == g.Key);
                return new
                {
                    ItemId = g.Key,
                    ItemCode = item?.ItemCode,
                    ItemName = item?.ItemName ?? g.First().Description,
                    Quantity = Math.Round(g.Sum(x => x.Quantity), 4),
                    Amount = Math.Round(g.Sum(x => x.LineTotal), 2),
                    AverageRate = g.Sum(x => x.Quantity) == 0
                        ? 0m
                        : Math.Round(g.Sum(x => x.LineTotal) / g.Sum(x => x.Quantity), 4)
                };
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
        return new
        {
            ReportCode = "SALE_SUMMARY_ITEM",
            FromDate = from,
            ToDate = to,
            TotalCount = summary.Count,
            TotalAmount = Math.Round(summary.Sum(x => x.Amount), 2),
            Lines = summary
        };
    }

    private async Task<object> SaleSummaryCustomerItemAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>() };
        var invQ = _db.SalesInvoices.AsNoTracking().Where(i => i.TenantId == tenantId);
        if (from.HasValue) invQ = invQ.Where(i => i.InvoiceDate >= from.Value.Date);
        if (to.HasValue) invQ = invQ.Where(i => i.InvoiceDate <= to.Value.Date);
        var invoices = await invQ.ToListAsync();
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var lines = await _db.SalesInvoiceLines.AsNoTracking()
            .Where(l => invoiceIds.Contains(l.SalesInvoiceId))
            .ToListAsync();
        var joined = from l in lines
                     join i in invoices on l.SalesInvoiceId equals i.Id
                     group new { l, i } by new { i.CustomerId, l.ItemId } into g
                     select new
                     {
                         g.Key.CustomerId,
                         g.Key.ItemId,
                         Quantity = Math.Round(g.Sum(x => x.l.Quantity), 4),
                         Amount = Math.Round(g.Sum(x => x.l.LineTotal), 2)
                     };
        var matrix = joined.OrderByDescending(x => x.Amount).Take(5_000).ToList();
        return new
        {
            ReportCode = "SALE_SUMMARY_CUSTOMER_ITEM",
            FromDate = from,
            ToDate = to,
            TotalCount = matrix.Count,
            TotalAmount = Math.Round(matrix.Sum(x => x.Amount), 2),
            Lines = matrix
        };
    }

    private async Task<object> StockOnHandAsync(Guid tenantId)
    {
        if (_db == null) return new { Lines = Array.Empty<object>() };
        var rows = await _db.StockBalances.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CurrentQuantity)
            .Take(5_000)
            .ToListAsync();
        var itemIds = rows.Select(r => r.ItemId).Distinct().ToList();
        var items = await _db.Items.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync();
        var lines = rows.Select(s =>
        {
            var item = items.FirstOrDefault(i => i.Id == s.ItemId);
            var avg = item?.CostPrice ?? 0m;
            return new
            {
                s.ItemId,
                s.WarehouseId,
                ItemCode = item?.ItemCode,
                ItemName = item?.ItemName,
                QuantityOnHand = s.CurrentQuantity,
                AvailableQuantity = s.AvailableQuantity,
                AverageCost = avg,
                Value = Math.Round(s.CurrentQuantity * avg, 2)
            };
        });
        return new
        {
            ReportCode = "STOCK_ON_HAND",
            TotalCount = rows.Count,
            TotalValue = Math.Round(rows.Sum(s =>
            {
                var item = items.FirstOrDefault(i => i.Id == s.ItemId);
                return s.CurrentQuantity * (item?.CostPrice ?? 0m);
            }), 2),
            Lines = lines
        };
    }

    private async Task<object> PurchaseOrderSummaryAsync(Guid tenantId, DateTime? from, DateTime? to)
    {
        if (_db == null) return new { Lines = Array.Empty<object>() };
        var q = _db.PurchaseOrders.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (from.HasValue) q = q.Where(p => p.OrderDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(p => p.OrderDate <= to.Value.Date);
        var rows = await q.ToListAsync();
        var byStatus = rows.GroupBy(p => p.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = Math.Round(g.Sum(x => x.TotalAmount), 2)
            })
            .OrderBy(x => x.Status)
            .ToList();
        return new
        {
            ReportCode = "PURCHASE_ORDER_SUMMARY",
            FromDate = from,
            ToDate = to,
            OrderCount = rows.Count,
            TotalAmount = Math.Round(rows.Sum(p => p.TotalAmount), 2),
            ByStatus = byStatus
        };
    }

    private static (DateTime? from, DateTime? to) ParseDateRange(Dictionary<string, object>? parameters)
    {
        if (parameters == null) return (null, null);
        DateTime? from = null, to = null;
        if (parameters.TryGetValue("fromDate", out var f) || parameters.TryGetValue("FromDate", out f)
            || parameters.TryGetValue("startDate", out f))
            from = ToDate(f);
        if (parameters.TryGetValue("toDate", out var t) || parameters.TryGetValue("ToDate", out t)
            || parameters.TryGetValue("endDate", out t) || parameters.TryGetValue("asOfDate", out t))
            to = ToDate(t);
        return (from, to);
    }

    private static DateTime? ToDate(object? value)
    {
        if (value == null) return null;
        if (value is DateTime dt) return dt.Date;
        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String && DateTime.TryParse(je.GetString(), out var parsed))
                return parsed.Date;
            if (je.ValueKind == JsonValueKind.Number && je.TryGetDateTime(out var n))
                return n.Date;
        }
        if (DateTime.TryParse(value.ToString(), out var d)) return d.Date;
        return null;
    }

    private static int CountRecords(object payload)
    {
        var type = payload.GetType();
        var prop = type.GetProperty("TotalCount") ?? type.GetProperty("InvoiceCount")
                   ?? type.GetProperty("OrderCount") ?? type.GetProperty("Lines");
        if (prop == null) return 0;
        var val = prop.GetValue(payload);
        if (val is int i) return i;
        if (val is System.Collections.ICollection c) return c.Count;
        if (val is System.Collections.IEnumerable e) return e.Cast<object>().Count();
        return 0;
    }

    private static ReportDto MapToDto(Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            Name = report.Name,
            ReportName = string.IsNullOrWhiteSpace(report.ReportName) ? report.Name : report.ReportName,
            ReportCode = report.ReportCode,
            Description = report.Description,
            ReportType = report.ReportType,
            Category = report.Category,
            // Never surface Query/SQL to clients — ReportCode is the whitelist key.
            Query = string.Empty,
            Parameters = report.Parameters ?? string.Empty,
            OutputFormat = report.OutputFormat,
            IsActive = report.IsActive,
            TenantId = report.TenantId,
            CreatedBy = report.CreatedBy,
            CreatedAt = report.CreatedAt,
            UpdatedBy = report.UpdatedBy,
            UpdatedAt = report.UpdatedAt
        };
    }

    private static ReportExecutionDto MapToDto(ReportExecution execution)
    {
        return new ReportExecutionDto
        {
            Id = execution.Id,
            ReportId = execution.ReportId,
            ScheduleId = execution.ScheduleId,
            Parameters = execution.Parameters,
            Status = execution.Status,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            ErrorMessage = execution.ErrorMessage,
            ResultData = execution.ResultData,
            OutputPath = execution.OutputPath,
            ExecutionTimeMs = execution.ExecutionTimeMs,
            RecordCount = execution.RecordCount,
            TenantId = execution.TenantId,
            ExecutedBy = execution.ExecutedBy,
            CreatedAt = execution.CreatedAt
        };
    }

    private static ReportScheduleDto MapToDto(ReportSchedule schedule)
    {
        return new ReportScheduleDto
        {
            Id = schedule.Id,
            ReportId = schedule.ReportId,
            ScheduleName = schedule.ScheduleName,
            ScheduleType = schedule.ScheduleType,
            CronExpression = schedule.CronExpression,
            NextRunTime = schedule.NextRunTime,
            LastRunTime = schedule.LastRunTime,
            IsActive = schedule.IsActive,
            EmailRecipients = schedule.EmailRecipients,
            Notes = schedule.Notes,
            ReportName = schedule.Report?.ReportName ?? schedule.Report?.Name,
            TenantId = schedule.TenantId,
            CreatedBy = schedule.CreatedBy,
            CreatedAt = schedule.CreatedAt,
            UpdatedBy = schedule.UpdatedBy,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}