// Quote Entity Types
// Based on ERP.Shared.DTOs.CRM.QuoteDto

export interface QuoteLineDto {
  id: string;
  quoteId: string;
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

export interface QuoteDto {
  id?: string;
  tenantId?: string;
  quoteNumber?: string;
  customerId?: string;
  opportunityId?: string;
  quoteDate?: string;
  validUntil?: string;
  status?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  currency?: string;
  notes?: string;
  terms?: string;
  sentDate?: string;
  convertedDate?: string;
  assignedToUserId?: string;
  isActive?: boolean;
  customerName?: string;
  opportunityName?: string;
  assignedToUserName?: string;
  companyId?: string;
  quoteLines?: QuoteLineDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateQuoteLineDto {
  itemId: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  discountPercent?: number;
  taxRate?: number;
  notes?: string;
}

export interface CreateQuoteDto {
  quoteNumber?: string;
  customerId?: string;
  opportunityId?: string;
  quoteDate?: string;
  validUntil?: string;
  status?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  currency?: string;
  notes?: string;
  terms?: string;
  assignedToUserId?: string;
  companyId?: string;
  quoteLines?: CreateQuoteLineDto[];
}

export interface UpdateQuoteDto {
  id: string;
  quoteNumber?: string;
  customerId?: string;
  opportunityId?: string;
  quoteDate?: string;
  validUntil?: string;
  status?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  currency?: string;
  notes?: string;
  terms?: string;
  assignedToUserId?: string;
  quoteLines?: CreateQuoteLineDto[];
}

// Quote Status Options
export const QUOTE_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Sent", label: "Sent" },
  { value: "Accepted", label: "Accepted" },
  { value: "Rejected", label: "Rejected" },
  { value: "Expired", label: "Expired" },
  { value: "Converted", label: "Converted" },
];

