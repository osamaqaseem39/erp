using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class FixedAssetMapper : BaseMapper<FixedAsset, FixedAssetDto>
{
    public override FixedAssetDto Map(FixedAsset source)
    {
        if (source == null) return null!;

        return new FixedAssetDto
        {
            Id = source.Id,
            AssetId = source.AssetId,
            AssetCode = source.AssetCode,
            AssetName = source.AssetName,
            AssetType = source.AssetCategory,
            GLAccountId = source.AccountId,
            PurchasePrice = source.PurchaseCost,
            PurchaseDate = source.PurchaseDate,
            Status = source.Status,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override FixedAsset Map(FixedAssetDto destination)
    {
        if (destination == null) return null!;

        return new FixedAsset
        {
            Id = destination.Id,
            AssetId = destination.AssetId ?? Guid.NewGuid(),
            AssetCode = destination.AssetCode,
            AssetName = destination.AssetName,
            AssetCategory = destination.AssetType,
            AccountId = destination.GLAccountId ?? Guid.Empty,
            PurchaseCost = destination.PurchasePrice,
            CurrentValue = destination.BookValue,
            PurchaseDate = destination.PurchaseDate,
            DisposalDate = destination.DisposedAt,
            Status = destination.Status,
            CurrencyCode = "USD",
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

