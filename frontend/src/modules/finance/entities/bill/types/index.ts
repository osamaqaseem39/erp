// Bill Entity Types
// Based on ERP.Shared.DTOs.Finance.BillDto

export interface BillLineDto {
  id: string;
  billLineId: string;
  billId: string;
  itemId?: string;
  itemCode?: string;
  itemName?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
  glAccountId?: string;
  costCenterId?: string;
  projectId?: string;
}

export interface BillDto {
  id?: string;
  tenantId?: string;
  billId: string;
  companyId: string;
  billNumber: string;
  vendorId: string;
  vendorName?: string;
  billDate: string;
  dueDate: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  status: string;
  notes?: string;
  approvedDate?: string;
  paidDate?: string;
  isActive: boolean;
  currencyCode?: string;
  exchangeRate?: number;
  reference?: string;
  billLines?: BillLineDto[];
}

export interface CreateBillLineDto {
  itemId?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  taxRate?: number;
  glAccountId?: string;
  costCenterId?: string;
  projectId?: string;
}

export interface CreateBillDto {
  companyId: string;
  billNumber?: string;
  vendorId: string;
  billDate: string;
  dueDate: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  status?: string;
  notes?: string;
  currencyCode?: string;
  exchangeRate?: number;
  reference?: string;
  isActive?: boolean;
  billLines?: CreateBillLineDto[];
}

export interface UpdateBillDto {
  billId: string;
  billNumber?: string;
  vendorId?: string;
  billDate?: string;
  dueDate?: string;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  status?: string;
  notes?: string;
  currencyCode?: string;
  exchangeRate?: number;
  reference?: string;
  isActive?: boolean;
}

export interface BillQuery {
  tenantId?: string;
  vendorId?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Bill Status Options
export const BILL_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Issued", label: "Issued" },
  { value: "Approved", label: "Approved" },
  { value: "Paid", label: "Paid" },
  { value: "PartiallyPaid", label: "Partially Paid" },
  { value: "Overdue", label: "Overdue" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Void", label: "Void" },
];

