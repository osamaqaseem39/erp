using ERP.Shared.DTOs.Inventory;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.Inventory;

public class ItemType : ObjectType<ItemDto>
{
    protected override void Configure(IObjectTypeDescriptor<ItemDto> descriptor)
    {
        descriptor.Field(i => i.Id).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.Description).Type<StringType>();
        descriptor.Field(i => i.CategoryId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.CategoryName).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.UnitOfMeasureId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.UnitOfMeasureName).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.ItemType).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.HSNCode).Type<StringType>();
        descriptor.Field(i => i.BarCode).Type<StringType>();
        descriptor.Field(i => i.ReorderLevel).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.ReorderQuantity).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.DefaultWarehouseId).Type<IdType>();
        descriptor.Field(i => i.DefaultWarehouseName).Type<StringType>();
        descriptor.Field(i => i.CostPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.SellingPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.IsSerialized).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.IsBatchTracked).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.IsActive).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(i => i.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(i => i.CreatedBy).Type<StringType>();
        descriptor.Field(i => i.UpdatedBy).Type<StringType>();
    }
}

public class CreateItemInputType : InputObjectType<CreateItemDto>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateItemDto> descriptor)
    {
        descriptor.Field(i => i.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.Description).Type<StringType>();
        descriptor.Field(i => i.CategoryId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.UnitOfMeasureId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.ItemType).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.HSNCode).Type<StringType>();
        descriptor.Field(i => i.BarCode).Type<StringType>();
        descriptor.Field(i => i.ReorderLevel).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.ReorderQuantity).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.DefaultWarehouseId).Type<IdType>();
        descriptor.Field(i => i.CostPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.SellingPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.IsSerialized).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.IsBatchTracked).Type<NonNullType<BooleanType>>();
    }
}

public class UpdateItemInputType : InputObjectType<UpdateItemDto>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateItemDto> descriptor)
    {
        descriptor.Field(i => i.Id).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.Description).Type<StringType>();
        descriptor.Field(i => i.CategoryId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.UnitOfMeasureId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.ItemType).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.HSNCode).Type<StringType>();
        descriptor.Field(i => i.BarCode).Type<StringType>();
        descriptor.Field(i => i.ReorderLevel).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.ReorderQuantity).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.DefaultWarehouseId).Type<IdType>();
        descriptor.Field(i => i.CostPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.SellingPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(i => i.IsSerialized).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.IsBatchTracked).Type<NonNullType<BooleanType>>();
        descriptor.Field(i => i.IsActive).Type<NonNullType<BooleanType>>();
    }
}