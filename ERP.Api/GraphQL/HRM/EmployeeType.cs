using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.HRM;

public class EmployeeType : ObjectType<EmployeeDto>
{
    protected override void Configure(IObjectTypeDescriptor<EmployeeDto> descriptor)
    {
        descriptor.Field(e => e.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(e => e.TenantId).Type<NonNullType<UuidType>>();
        descriptor.Field(e => e.CompanyId).Type<UuidType>();
        descriptor.Field(e => e.EmployeeCode).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.FirstName).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.LastName).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.JobTitle).Type<StringType>();
        descriptor.Field(e => e.WorkEmail).Type<StringType>();
        descriptor.Field(e => e.Status).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.DateHired).Type<DateTimeType>();
        descriptor.Field(e => e.DateTerminated).Type<DateTimeType>();
        descriptor.Field(e => e.IsRemote).Type<NonNullType<BooleanType>>();
        descriptor.Field(e => e.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(e => e.UpdatedAt).Type<DateTimeType>();
    }
}

public class EmployeeListType : ObjectType<EmployeeListDto>
{
    protected override void Configure(IObjectTypeDescriptor<EmployeeListDto> descriptor)
    {
        descriptor.Field(e => e.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(e => e.EmployeeCode).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.FirstName).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.LastName).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.JobTitle).Type<StringType>();
        descriptor.Field(e => e.Department).Type<StringType>();
        descriptor.Field(e => e.WorkEmail).Type<StringType>();
        descriptor.Field(e => e.Status).Type<NonNullType<StringType>>();
        descriptor.Field(e => e.DateHired).Type<DateTimeType>();
    }
}

public class EmployeeConnectionType : ObjectType<PagedResult<EmployeeListDto>>
{
    protected override void Configure(IObjectTypeDescriptor<PagedResult<EmployeeListDto>> descriptor)
    {
        descriptor.Field(c => c.Items).Type<NonNullType<ListType<NonNullType<EmployeeListType>>>>();
        descriptor.Field(c => c.TotalCount).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.PageNumber).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.PageSize).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.TotalPages).Type<NonNullType<IntType>>();
    }
}