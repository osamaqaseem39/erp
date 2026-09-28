// Payment Entity Types
// Based on ERP.Shared.DTOs.Finance.PaymentDto

export interface PaymentDto {
  id?: string;
  tenantId?: string;
  paymentId?: string;
  paymentNumber?: string;
  paymentDate?: string;
  paymentType?: string;
  paymentMethod?: string;
  amount?: number;
  currency?: string;
  currencyCode?: string;
  bankAccountId?: string;
  bankAccountName?: string;
  relatedInvoiceId?: string;
  relatedInvoiceNumber?: string;
  relatedBillId?: string;
  relatedBillNumber?: string;
  status?: string;
  reference?: string;
  referenceNumber?: string;
  notes?: string;
  processedDate?: string;
  processedAt?: string;
  processedBy?: string;
  isActive?: boolean;
  companyId?: string;
}

export interface CreatePaymentDto {
  paymentNumber?: string;
  paymentDate?: string;
  paymentType?: string;
  paymentMethod?: string;
  amount?: number;
  currency?: string;
  currencyCode?: string;
  bankAccountId?: string;
  relatedInvoiceId?: string;
  relatedBillId?: string;
  status?: string;
  reference?: string;
  referenceNumber?: string;
  notes?: string;
  companyId?: string;
}

export interface UpdatePaymentDto {
  paymentId?: string;
  paymentNumber?: string;
  paymentDate?: string;
  paymentType?: string;
  paymentMethod?: string;
  amount?: number;
  currency?: string;
  currencyCode?: string;
  bankAccountId?: string;
  relatedInvoiceId?: string;
  relatedBillId?: string;
  status?: string;
  reference?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface PaymentQuery {
  tenantId?: string;
  status?: string;
  paymentMethod?: string;
  fromDate?: string;
  toDate?: string;
  customerId?: string;
  invoiceId?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Payment Type Options
export const PAYMENT_TYPE_OPTIONS = [
  { value: "Invoice", label: "Invoice Payment" },
  { value: "Bill", label: "Bill Payment" },
  { value: "Refund", label: "Refund" },
  { value: "Other", label: "Other" },
];

// Payment Method Options
export const PAYMENT_METHOD_OPTIONS = [
  { value: "Cash", label: "Cash" },
  { value: "Bank", label: "Bank Transfer" },
  { value: "Card", label: "Card" },
  { value: "Check", label: "Check" },
  { value: "Mobile", label: "Mobile Payment" },
  { value: "Other", label: "Other" },
];

// Payment Status Options
export const PAYMENT_STATUS_OPTIONS = [
  { value: "Pending", label: "Pending" },
  { value: "Completed", label: "Completed" },
  { value: "Failed", label: "Failed" },
  { value: "Reconciled", label: "Reconciled" },
  { value: "Cancelled", label: "Cancelled" },
];

