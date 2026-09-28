// POS Transaction Entity Types
// Based on ERP.Shared.DTOs.Sales.POSTransactionDto

export interface POSTransactionLineDto {
  id: string;
  posTransactionId: string;
  itemId: string;
  itemName?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
  lineNumber: number;
}

export interface POSTransactionDto {
  id?: string;
  tenantId?: string;
  transactionNumber: string;
  posSessionId: string;
  sessionId: string;
  customerId: string;
  customerName?: string;
  transactionDate: string; // ISO date string
  paymentMethod: string; // "Cash" | "Card" | "Mobile" | "Other"
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  amountPaid: number;
  changeAmount: number;
  status: string; // "Completed" | "Pending" | "Cancelled" | "Refunded"
  notes?: string;
  lines: POSTransactionLineDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePOSTransactionLineDto {
  itemId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
  lineNumber: number;
}

export interface CreatePOSTransactionDto {
  transactionNumber: string;
  posSessionId: string;
  sessionId: string;
  customerId: string;
  transactionDate: string; // ISO date string
  paymentMethod: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  discountAmount: number;
  amountPaid: number;
  status: string;
  notes?: string;
  lines: CreatePOSTransactionLineDto[];
}

export interface UpdatePOSTransactionLineDto {
  id: string;
  itemId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
  lineNumber: number;
}

export interface UpdatePOSTransactionDto {
  id: string;
  transactionNumber: string;
  sessionId: string;
  customerId: string;
  transactionDate: string; // ISO date string
  paymentMethod: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  discountAmount: number;
  amountPaid: number;
  status: string;
  notes?: string;
  lines: UpdatePOSTransactionLineDto[];
}

// Enum values for dropdowns
export const PAYMENT_METHOD_OPTIONS = [
  { value: "Cash", label: "Cash" },
  { value: "Card", label: "Card" },
  { value: "Mobile", label: "Mobile Payment" },
  { value: "Other", label: "Other" },
];

export const TRANSACTION_STATUS_OPTIONS = [
  { value: "Completed", label: "Completed" },
  { value: "Pending", label: "Pending" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Refunded", label: "Refunded" },
];

