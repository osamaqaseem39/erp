using ERP.Shared.DTOs.Procurement;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.Procurement;

public class VendorType : ObjectType<VendorDto>
{
    protected override void Configure(IObjectTypeDescriptor<VendorDto> descriptor)
    {
        descriptor.Field(v => v.Id).Type<NonNullType<IdType>>();
        descriptor.Field(v => v.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(v => v.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(v => v.CompanyName).Type<NonNullType<StringType>>();
        descriptor.Field(v => v.VendorCode).Type<NonNullType<StringType>>();
        descriptor.Field(v => v.VendorName).Type<NonNullType<StringType>>();
        descriptor.Field(v => v.ContactPerson).Type<StringType>();
        descriptor.Field(v => v.PhoneNumber).Type<StringType>();
        descriptor.Field(v => v.Email).Type<StringType>();
        descriptor.Field(v => v.AddressLine1).Type<StringType>();
        descriptor.Field(v => v.AddressLine2).Type<StringType>();
        descriptor.Field(v => v.City).Type<StringType>();
        descriptor.Field(v => v.State).Type<StringType>();
        descriptor.Field(v => v.Country).Type<StringType>();
        descriptor.Field(v => v.PostalCode).Type<StringType>();
        descriptor.Field(v => v.TaxNumber).Type<StringType>();
        descriptor.Field(v => v.PaymentTerms).Type<StringType>();
        descriptor.Field(v => v.CreditLimit).Type<NonNullType<DecimalType>>();
        descriptor.Field(v => v.IsActive).Type<NonNullType<BooleanType>>();
        descriptor.Field(v => v.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(v => v.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(v => v.CreatedBy).Type<StringType>();
        descriptor.Field(v => v.UpdatedBy).Type<StringType>();
        descriptor.Field(v => v.BankAccounts).Type<ListType<VendorBankAccountType>>();
    }
}

public class VendorBankAccountType : ObjectType<VendorBankAccountDto>
{
    protected override void Configure(IObjectTypeDescriptor<VendorBankAccountDto> descriptor)
    {
        descriptor.Field(vba => vba.Id).Type<NonNullType<IdType>>();
        descriptor.Field(vba => vba.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(vba => vba.VendorId).Type<NonNullType<IdType>>();
        descriptor.Field(vba => vba.BankName).Type<NonNullType<StringType>>();
        descriptor.Field(vba => vba.AccountNumber).Type<NonNullType<StringType>>();
        descriptor.Field(vba => vba.IBAN).Type<StringType>();
        descriptor.Field(vba => vba.SWIFTCode).Type<StringType>();
        descriptor.Field(vba => vba.Currency).Type<NonNullType<StringType>>();
        descriptor.Field(vba => vba.IsPrimary).Type<NonNullType<BooleanType>>();
        descriptor.Field(vba => vba.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(vba => vba.CreatedBy).Type<StringType>();
    }
}

public class PurchaseRequisitionType : ObjectType<PurchaseRequisitionDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseRequisitionDto> descriptor)
    {
        descriptor.Field(pr => pr.Id).Type<NonNullType<IdType>>();
        descriptor.Field(pr => pr.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(pr => pr.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(pr => pr.CompanyName).Type<NonNullType<StringType>>();
        descriptor.Field(pr => pr.RequestedByUserId).Type<NonNullType<IdType>>();
        descriptor.Field(pr => pr.RequestedByName).Type<NonNullType<StringType>>();
        descriptor.Field(pr => pr.DepartmentId).Type<IdType>();
        descriptor.Field(pr => pr.RequestDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pr => pr.RequiredByDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pr => pr.Status).Type<NonNullType<StringType>>();
        descriptor.Field(pr => pr.Remarks).Type<StringType>();
        descriptor.Field(pr => pr.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pr => pr.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(pr => pr.CreatedBy).Type<StringType>();
        descriptor.Field(pr => pr.UpdatedBy).Type<StringType>();
        descriptor.Field(pr => pr.Details).Type<ListType<PurchaseRequisitionDetailType>>();
    }
}

public class PurchaseRequisitionDetailType : ObjectType<PurchaseRequisitionDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseRequisitionDetailDto> descriptor)
    {
        descriptor.Field(prd => prd.Id).Type<NonNullType<IdType>>();
        descriptor.Field(prd => prd.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(prd => prd.PRId).Type<NonNullType<IdType>>();
        descriptor.Field(prd => prd.ItemId).Type<NonNullType<IdType>>();
        descriptor.Field(prd => prd.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(prd => prd.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(prd => prd.QuantityRequested).Type<NonNullType<DecimalType>>();
        descriptor.Field(prd => prd.UnitOfMeasureId).Type<NonNullType<IdType>>();
        descriptor.Field(prd => prd.UnitOfMeasureName).Type<NonNullType<StringType>>();
        descriptor.Field(prd => prd.UnitSymbol).Type<NonNullType<StringType>>();
        descriptor.Field(prd => prd.EstimatedUnitPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(prd => prd.Remarks).Type<StringType>();
        descriptor.Field(prd => prd.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(prd => prd.CreatedBy).Type<StringType>();
    }
}

public class PurchaseOrderType : ObjectType<PurchaseOrderDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseOrderDto> descriptor)
    {
        descriptor.Field(po => po.Id).Type<NonNullType<IdType>>();
        descriptor.Field(po => po.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(po => po.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(po => po.CompanyName).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.RFQId).Type<IdType>();
        descriptor.Field(po => po.VendorId).Type<NonNullType<IdType>>();
        descriptor.Field(po => po.VendorName).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.VendorCode).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.OrderDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(po => po.ExpectedDeliveryDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(po => po.Currency).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.TotalAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(po => po.PaymentTerms).Type<StringType>();
        descriptor.Field(po => po.DeliveryTerms).Type<StringType>();
        descriptor.Field(po => po.Status).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.CreatedBy).Type<NonNullType<StringType>>();
        descriptor.Field(po => po.ApprovedBy).Type<StringType>();
        descriptor.Field(po => po.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(po => po.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(po => po.UpdatedBy).Type<StringType>();
        descriptor.Field(po => po.Details).Type<ListType<PurchaseOrderDetailType>>();
    }
}

public class PurchaseOrderDetailType : ObjectType<PurchaseOrderDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseOrderDetailDto> descriptor)
    {
        descriptor.Field(pod => pod.Id).Type<NonNullType<IdType>>();
        descriptor.Field(pod => pod.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(pod => pod.POId).Type<NonNullType<IdType>>();
        descriptor.Field(pod => pod.ItemId).Type<NonNullType<IdType>>();
        descriptor.Field(pod => pod.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(pod => pod.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(pod => pod.QuantityOrdered).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.UnitPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.Discount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.TaxAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.NetAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.ReceivedQuantity).Type<NonNullType<DecimalType>>();
        descriptor.Field(pod => pod.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pod => pod.CreatedBy).Type<StringType>();
    }
}

public class GoodsReceiptNoteType : ObjectType<GoodsReceiptNoteDto>
{
    protected override void Configure(IObjectTypeDescriptor<GoodsReceiptNoteDto> descriptor)
    {
        descriptor.Field(grn => grn.Id).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.CompanyName).Type<NonNullType<StringType>>();
        descriptor.Field(grn => grn.POId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.PurchaseOrderNumber).Type<StringType>();
        descriptor.Field(grn => grn.VendorId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.VendorName).Type<NonNullType<StringType>>();
        descriptor.Field(grn => grn.ReceivedDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(grn => grn.WarehouseId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.WarehouseName).Type<NonNullType<StringType>>();
        descriptor.Field(grn => grn.ReceivedByUserId).Type<NonNullType<IdType>>();
        descriptor.Field(grn => grn.ReceivedByName).Type<NonNullType<StringType>>();
        descriptor.Field(grn => grn.Status).Type<NonNullType<StringType>>();
        descriptor.Field(grn => grn.Remarks).Type<StringType>();
        descriptor.Field(grn => grn.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(grn => grn.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(grn => grn.CreatedBy).Type<StringType>();
        descriptor.Field(grn => grn.UpdatedBy).Type<StringType>();
        descriptor.Field(grn => grn.Details).Type<ListType<GoodsReceiptNoteDetailType>>();
    }
}

public class GoodsReceiptNoteDetailType : ObjectType<GoodsReceiptNoteDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<GoodsReceiptNoteDetailDto> descriptor)
    {
        descriptor.Field(grnd => grnd.Id).Type<NonNullType<IdType>>();
        descriptor.Field(grnd => grnd.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(grnd => grnd.GRNId).Type<NonNullType<IdType>>();
        descriptor.Field(grnd => grnd.ItemId).Type<NonNullType<IdType>>();
        descriptor.Field(grnd => grnd.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(grnd => grnd.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(grnd => grnd.QuantityReceived).Type<NonNullType<DecimalType>>();
        descriptor.Field(grnd => grnd.BatchSerialId).Type<IdType>();
        descriptor.Field(grnd => grnd.BatchNumber).Type<StringType>();
        descriptor.Field(grnd => grnd.SerialNumber).Type<StringType>();
        descriptor.Field(grnd => grnd.UnitPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(grnd => grnd.TaxAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(grnd => grnd.TotalAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(grnd => grnd.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(grnd => grnd.CreatedBy).Type<StringType>();
    }
}

public class PurchaseInvoiceType : ObjectType<PurchaseInvoiceDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseInvoiceDto> descriptor)
    {
        descriptor.Field(pi => pi.Id).Type<NonNullType<IdType>>();
        descriptor.Field(pi => pi.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(pi => pi.CompanyId).Type<NonNullType<IdType>>();
        descriptor.Field(pi => pi.CompanyName).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.POId).Type<IdType>();
        descriptor.Field(pi => pi.PurchaseOrderNumber).Type<StringType>();
        descriptor.Field(pi => pi.VendorId).Type<NonNullType<IdType>>();
        descriptor.Field(pi => pi.VendorName).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.VendorCode).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.InvoiceNumber).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.InvoiceDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pi => pi.DueDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pi => pi.Currency).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.TotalAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pi => pi.TaxAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pi => pi.NetPayable).Type<NonNullType<DecimalType>>();
        descriptor.Field(pi => pi.Status).Type<NonNullType<StringType>>();
        descriptor.Field(pi => pi.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pi => pi.UpdatedAt).Type<DateTimeType>();
        descriptor.Field(pi => pi.CreatedBy).Type<StringType>();
        descriptor.Field(pi => pi.UpdatedBy).Type<StringType>();
        descriptor.Field(pi => pi.Details).Type<ListType<PurchaseInvoiceDetailType>>();
    }
}

public class PurchaseInvoiceDetailType : ObjectType<PurchaseInvoiceDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseInvoiceDetailDto> descriptor)
    {
        descriptor.Field(pid => pid.Id).Type<NonNullType<IdType>>();
        descriptor.Field(pid => pid.TenantId).Type<NonNullType<IdType>>();
        descriptor.Field(pid => pid.InvoiceId).Type<NonNullType<IdType>>();
        descriptor.Field(pid => pid.ItemId).Type<NonNullType<IdType>>();
        descriptor.Field(pid => pid.ItemName).Type<NonNullType<StringType>>();
        descriptor.Field(pid => pid.ItemCode).Type<NonNullType<StringType>>();
        descriptor.Field(pid => pid.Quantity).Type<NonNullType<DecimalType>>();
        descriptor.Field(pid => pid.UnitPrice).Type<NonNullType<DecimalType>>();
        descriptor.Field(pid => pid.TaxAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pid => pid.NetAmount).Type<NonNullType<DecimalType>>();
        descriptor.Field(pid => pid.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(pid => pid.CreatedBy).Type<StringType>();
    }
}