// Return Order Entity Types
// Based on ERP.Shared.DTOs.CRM.ReturnOrderDto

export interface ReturnOrderDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  returnNumber?: string;
  returnDate?: string;
  customerId?: string;
  customerName?: string;
  salesOrderId?: string;
  reason?: string;
  status?: string;
  totalAmount?: number;
  notes?: string;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateReturnOrderDto {
  companyId?: string;
  returnNumber?: string;
  returnDate?: string;
  customerId?: string;
  salesOrderId?: string;
  reason?: string;
  status?: string;
  totalAmount?: number;
  notes?: string;
}

export interface UpdateReturnOrderDto {
  id: string;
  reason?: string;
  status?: string;
  totalAmount?: number;
  notes?: string;
}

export const RETURN_ORDER_STATUS_OPTIONS = [
  { value: "Pending", label: "Pending" },
  { value: "Approved", label: "Approved" },
  { value: "Processing", label: "Processing" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];
