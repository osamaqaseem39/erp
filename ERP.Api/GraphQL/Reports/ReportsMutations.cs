using ERP.Application.Interfaces.Reports;
using ERP.Shared.DTOs.Reports;
using HotChocolate;

namespace ERP.Api.GraphQL.Reports;

[ExtendObjectType("Mutation")]
public class ReportsMutations
{
    // Report Mutations
    public async Task<ReportDto> AddReport(
        [Service] IReportService service,
        CreateReportDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<ReportDto> UpdateReport(
        [Service] IReportService service,
        UpdateReportDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteReport(
        [Service] IReportService service,
        Guid reportId,
        Guid tenantId)
    {
        return await service.DeleteAsync(reportId, tenantId);
    }

    // Report Execution Mutations
    public async Task<ReportExecutionDto> ExecuteReport(
        [Service] IReportService service,
        Guid reportId,
        Dictionary<string, object>? parameters,
        Guid tenantId,
        Guid userId)
    {
        return await service.ExecuteAsync(reportId, parameters, tenantId, userId);
    }

    public async Task<ReportExecutionDto> GenerateReport(
        [Service] IReportService service,
        GenerateReportDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.GenerateReportAsync(input, tenantId, userId);
    }

    // Report Schedule Mutations
    public async Task<ReportScheduleDto> ScheduleReport(
        [Service] IReportService service,
        ScheduleReportDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.ScheduleReportAsync(input, tenantId, userId);
    }

    public async Task<bool> CancelScheduledReport(
        [Service] IReportService service,
        Guid scheduleId,
        Guid tenantId)
    {
        return await service.CancelScheduledReportAsync(scheduleId, tenantId);
    }
}

