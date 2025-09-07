using ERP.Shared.DTOs.Inventory;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.Inventory;

public class WarehouseType : ObjectType<WarehouseDto>
{
    protected override void Configure(IObjectTypeDescriptor<WarehouseDto> descriptor)
    {
        descriptor.Field(w => w.Id).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.WarehouseCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.WarehouseName).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine1).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine2).Type<StringType>();
        descriptor.Field(w => w.City).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.State).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.Country).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.PostalCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.ContactPerson).Type<StringType>();
        descriptor.Field(w => w.PhoneNumber).Type<StringType>();
        descriptor.Field(w => w.Email).Type<StringType>();
        descriptor.Field(w => w.IsActive).Type<NonNullType<BooleanType>>();
        descriptor.Field(w => w.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(w => w.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(w => w.CreatedBy).Type<StringType>();
        descriptor.Field(w => w.UpdatedBy).Type<StringType>();
    }
}

public class CreateWarehouseInputType : InputObjectType<CreateWarehouseDto>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateWarehouseDto> descriptor)
    {
        descriptor.Field(w => w.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.WarehouseCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.WarehouseName).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine1).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine2).Type<StringType>();
        descriptor.Field(w => w.City).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.State).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.Country).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.PostalCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.ContactPerson).Type<StringType>();
        descriptor.Field(w => w.PhoneNumber).Type<StringType>();
        descriptor.Field(w => w.Email).Type<StringType>();
    }
}

public class UpdateWarehouseInputType : InputObjectType<UpdateWarehouseDto>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateWarehouseDto> descriptor)
    {
        descriptor.Field(w => w.Id).Type<NonNullType<IdType>>();
        descriptor.Field(w => w.WarehouseCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.WarehouseName).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine1).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.AddressLine2).Type<StringType>();
        descriptor.Field(w => w.City).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.State).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.Country).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.PostalCode).Type<NonNullType<StringType>>();
        descriptor.Field(w => w.ContactPerson).Type<StringType>();
        descriptor.Field(w => w.PhoneNumber).Type<StringType>();
        descriptor.Field(w => w.Email).Type<StringType>();
        descriptor.Field(w => w.IsActive).Type<NonNullType<BooleanType>>();
    }
}