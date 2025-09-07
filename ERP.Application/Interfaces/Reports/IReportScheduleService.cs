using ERP.Shared.DTOs.Reports;

namespace ERP.Application.Interfaces.Reports;

public interface IReportScheduleService
{
    Task<ReportScheduleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportScheduleDto>> GetAllAsync();
    Task<IEnumerable<ReportScheduleDto>> GetByReportAsync(Guid reportId);
    Task<IEnumerable<ReportScheduleDto>> GetActiveAsync();
    Task<Guid> CreateAsync(CreateReportScheduleDto dto);
    Task<bool> UpdateAsync(UpdateReportScheduleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
}