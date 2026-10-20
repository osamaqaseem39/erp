using ERP.Application.Core.Settings;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Core;

namespace ERP.Tests.TestHelpers;

/// <summary>Disables procurement three-way match so legacy PI approve tests stay valid.</summary>
public sealed class ProcurementTestSettings : ICoreSettingsService
{
    public Task<string?> GetValueAsync(string key) =>
        Task.FromResult<string?>(key switch
        {
            SettingsCatalog.ThreeWayMatchKey => "false",
            SettingsCatalog.ThreeWayMatchToleranceKey => "2",
            _ => null
        });

    public Task SetValueAsync(string key, string? value) => Task.CompletedTask;

    public Task<SettingsCategoryDto> GetCategoryAsync(string category) =>
        Task.FromResult(new SettingsCategoryDto { Category = category, Settings = [] });

    public Task<SettingsCategoryDto> UpdateCategoryAsync(string category, UpdateSettingsCategoryDto dto) =>
        GetCategoryAsync(category);
}

public static class ProcurementTestServices
{
    public static PurchaseInvoiceService CreatePurchaseInvoiceService(ERPDbContext db, IBillService bills) =>
        new(
            new PurchaseInvoiceRepository(db),
            new PurchaseInvoiceDetailRepository(db),
            bills,
            new PurchaseOrderDetailRepository(db),
            new GoodsReceiptNoteRepository(db),
            new ProcurementTestSettings());
}
