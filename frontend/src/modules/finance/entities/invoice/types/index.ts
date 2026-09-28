// Invoice Entity Types
// Based on ERP.Shared.DTOs.Finance.InvoiceDto

export interface InvoiceLineDto {
  id: string;
  invoiceId: string;
  productId?: string;
  productName?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  taxRate?: number;
  taxAmount?: number;
  glAccountId?: string;
  glAccountCode?: string;
  glAccountName?: string;
  notes?: string;
}

export interface InvoiceDto {
  id?: string;
  tenantId?: string;
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  customerName?: string;
  invoiceDate: string;
  dueDate: string;
  paymentDate?: string;
  currencyCode: string;
  subTotal: number;
  taxAmount: number;
  taxTotal: number;
  totalAmount: number;
  balance: number;
  paidAmount: number;
  balanceAmount: number;
  status: string;
  salesOrderId?: string;
  salesOrderNumber?: string;
  postedAt?: string;
  postedBy?: string;
  isLocked: boolean;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  invoiceLines?: InvoiceLineDto[];
}

export interface CreateInvoiceLineDto {
  productId?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  taxRate?: number;
  glAccountId?: string;
  notes?: string;
}

export interface CreateInvoiceDto {
  invoiceNumber?: string;
  customerId: string;
  invoiceDate?: string;
  dueDate?: string;
  currencyCode?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  status?: string;
  salesOrderId?: string;
  isLocked?: boolean;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
  invoiceLines?: CreateInvoiceLineDto[];
}

export interface UpdateInvoiceDto {
  invoiceId: string;
  invoiceNumber?: string;
  customerId?: string;
  invoiceDate?: string;
  dueDate?: string;
  currencyCode?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  paidAmount?: number;
  status?: string;
  salesOrderId?: string;
  isLocked?: boolean;
  notes?: string;
  terms?: string;
  shippingAddress?: string;
  billingAddress?: string;
}

export interface InvoiceQuery {
  tenantId?: string;
  customerId?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Invoice Status Options
export const INVOICE_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Sent", label: "Sent" },
  { value: "Paid", label: "Paid" },
  { value: "PartiallyPaid", label: "Partially Paid" },
  { value: "Overdue", label: "Overdue" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Void", label: "Void" },
];

