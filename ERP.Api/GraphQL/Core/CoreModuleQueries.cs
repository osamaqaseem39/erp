using ERP.Application.Admin;
using ERP.Application.Core;
using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Core;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using HotChocolate;

namespace ERP.Api.GraphQL.Core;

[ExtendObjectType("Query")]
public class CoreModuleQueries
{
    public async Task<FiscalYearDto?> GetFiscalYear(
        [Service] IFiscalYearService service,
        Guid fiscalYearId,
        Guid tenantId)
        => await service.GetByIdAsync(fiscalYearId, tenantId);

    public async Task<IEnumerable<FiscalYearDto>> GetFiscalYears(
        [Service] IFiscalYearService service,
        Guid tenantId,
        string? status = null)
        => await service.GetAllAsync(tenantId, status);

    public async Task<FiscalYearDto?> GetActiveFiscalYearCovering(
        [Service] IFiscalYearService service,
        Guid tenantId,
        DateTime date)
        => await service.GetActiveCoveringAsync(tenantId, date);

    // Company profile + tenant formatting context
    public async Task<CompanyProfileDto> GetCompanyProfile(
        [Service] ICompanyProfileService service,
        Guid companyId,
        Guid tenantId)
        => await service.GetAsync(companyId, tenantId);

    public async Task<TenantContextDto> GetTenantContext(
        [Service] ICompanyProfileService service,
        Guid tenantId,
        Guid? companyId = null)
        => await service.GetTenantContextAsync(companyId, tenantId);

    // Typed settings (preferences / security / email / finance)
    public async Task<SettingsCategoryDto> GetSettingsCategory(
        [Service] ICoreSettingsService service,
        string category)
        => await service.GetCategoryAsync(category);

    // Document numbering sequences
    public async Task<IEnumerable<DocumentSequenceDto>> GetDocumentSequences(
        [Service] IDocumentNumberService service,
        CancellationToken cancellationToken = default)
        => await service.ListAsync(cancellationToken);

    // Dashboards
    public async Task<CoreDashboardDto> GetCoreDashboard(
        [Service] ICoreDashboardService service,
        Guid tenantId,
        Guid? companyId = null,
        bool includePlatform = false,
        CancellationToken cancellationToken = default)
        => await service.GetSummaryAsync(tenantId, companyId, includePlatform, cancellationToken);

    public async Task<AdminDashboardDto> GetAdminDashboard(
        [Service] IAdminDashboardService service,
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => await service.GetSummaryAsync(tenantId, cancellationToken);
}
