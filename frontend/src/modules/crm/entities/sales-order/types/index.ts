// SalesOrder Entity Types
// Based on ERP.Shared.DTOs.CRM.SalesOrderDto

export interface SalesOrderLineDto {
  id: string;
  salesOrderId: string;
  itemId: string;
  itemName?: string;
  itemCode?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  totalPrice: number;
  notes?: string;
}

export interface SalesOrderDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  orderNumber: string;
  salesOrderNumber: string;
  customerId?: string;
  quoteId?: string;
  orderDate: string;
  requiredDate?: string;
  shippedDate?: string;
  approvedDate?: string;
  invoicedDate?: string;
  status: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  currency?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
  assignedToUserId?: string;
  isActive: boolean;
  customerName?: string;
  quoteNumber?: string;
  assignedToUserName?: string;
  salesOrderLines?: SalesOrderLineDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSalesOrderLineDto {
  itemId: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  discountPercent?: number;
  taxRate?: number;
  notes?: string;
}

export interface CreateSalesOrderDto {
  companyId: string;
  orderNumber: string;
  salesOrderNumber: string;
  customerId?: string;
  quoteId?: string;
  orderDate: string;
  requiredDate?: string;
  shippedDate?: string;
  status?: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  currency?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
  assignedToUserId?: string;
  salesOrderLines?: CreateSalesOrderLineDto[];
}

export interface UpdateSalesOrderDto {
  id: string;
  orderNumber?: string;
  salesOrderNumber?: string;
  customerId?: string;
  quoteId?: string;
  orderDate?: string;
  requiredDate?: string;
  shippedDate?: string;
  status?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  currency?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
  assignedToUserId?: string;
  salesOrderLines?: CreateSalesOrderLineDto[];
}

// Sales Order Status Options
export const SALES_ORDER_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Pending", label: "Pending" },
  { value: "Approved", label: "Approved" },
  { value: "Shipped", label: "Shipped" },
  { value: "Invoiced", label: "Invoiced" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

