// PurchaseOrder Entity Types
// Based on ERP.Shared.DTOs.Procurement.PurchaseOrderDto

export interface PurchaseOrderDetailDto {
  id: string;
  tenantId: string;
  poId?: string;
  purchaseOrderId: string;
  itemId: string;
  itemName?: string;
  itemCode?: string;
  quantity: number;
  quantityOrdered: number;
  unitPrice: number;
  discount?: number;
  taxAmount?: number;
  netAmount?: number;
  totalPrice: number;
  receivedQuantity?: number;
  createdAt: string;
  createdBy?: string;
  notes?: string;
}

export interface PurchaseOrderDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  companyName?: string;
  purchaseOrderNumber: string;
  vendorId: string;
  vendorName?: string;
  vendorCode?: string;
  rfqId?: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  actualDeliveryDate?: string;
  currency?: string;
  paymentTerms?: string;
  deliveryTerms?: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  status: string;
  approvedBy?: string;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  details?: PurchaseOrderDetailDto[];
  purchaseOrderDetails?: PurchaseOrderDetailDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePurchaseOrderDetailDto {
  itemId: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
}

export interface CreatePurchaseOrderDto {
  purchaseOrderNumber?: string;
  vendorId: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  status?: string;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  details?: CreatePurchaseOrderDetailDto[];
  purchaseOrderDetails?: CreatePurchaseOrderDetailDto[];
}

export interface UpdatePurchaseOrderDto {
  id: string;
  purchaseOrderNumber?: string;
  vendorId?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  actualDeliveryDate?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  status?: string;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  details?: CreatePurchaseOrderDetailDto[];
  purchaseOrderDetails?: CreatePurchaseOrderDetailDto[];
}

// Purchase Order Status Options
export const PURCHASE_ORDER_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Issued", label: "Issued" },
  { value: "PartiallyReceived", label: "Partially Received" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
];

