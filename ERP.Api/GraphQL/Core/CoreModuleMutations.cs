using ERP.Application.Core;
using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Core;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.Core;
using HotChocolate;

namespace ERP.Api.GraphQL.Core;

[ExtendObjectType("Mutation")]
public class CoreModuleMutations
{
    public async Task<Guid> CreateFiscalYear(
        [Service] IFiscalYearService service,
        CreateFiscalYearDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<Guid> UpdateFiscalYear(
        [Service] IFiscalYearService service,
        UpdateFiscalYearDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> CloseFiscalYear(
        [Service] IFiscalYearService service,
        Guid fiscalYearId,
        Guid tenantId,
        Guid userId)
        => await service.CloseAsync(fiscalYearId, tenantId, userId);

    public async Task<bool> DeleteFiscalYear(
        [Service] IFiscalYearService service,
        Guid fiscalYearId,
        Guid tenantId)
        => await service.DeleteAsync(fiscalYearId, tenantId);

    public async Task<CompanyProfileDto> UpdateCompanyProfile(
        [Service] ICompanyProfileService service,
        Guid companyId,
        Guid tenantId,
        UpdateCompanyProfileDto input)
        => await service.UpdateAsync(companyId, tenantId, input);

    public async Task<SettingsCategoryDto> UpdateSettingsCategory(
        [Service] ICoreSettingsService service,
        string category,
        UpdateSettingsCategoryDto input)
        => await service.UpdateCategoryAsync(category, input);

    public async Task<DocumentSequenceDto> UpdateDocumentSequence(
        [Service] IDocumentNumberService service,
        string key,
        UpdateDocumentSequenceDto input,
        CancellationToken cancellationToken = default)
        => await service.UpdateAsync(key, input, cancellationToken);
}
