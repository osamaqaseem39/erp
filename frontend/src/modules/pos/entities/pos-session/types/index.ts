// POS Session Entity Types
// Based on ERP.Shared.DTOs.Sales.POSSessionDto

export interface POSSessionDto {
  id?: string;
  tenantId?: string;
  sessionNumber: string;
  cashierId: string;
  cashierName?: string;
  terminalId: string;
  terminalName?: string;
  startTime: string; // ISO date string
  endTime?: string; // ISO date string
  openingBalance: number;
  closingBalance: number;
  expectedBalance: number;
  difference: number;
  status: string; // "Open" | "Closed" | "Suspended"
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePOSSessionDto {
  sessionNumber: string;
  cashierId: string;
  terminalId: string;
  startTime: string; // ISO date string
  openingBalance: number;
  status: string;
  notes?: string;
}

export interface UpdatePOSSessionDto {
  id: string;
  sessionNumber: string;
  cashierId: string;
  terminalId: string;
  startTime?: string; // ISO date string
  endTime?: string; // ISO date string
  openingBalance?: number;
  closingBalance: number;
  status: string;
  notes?: string;
}

export interface ClosePOSSessionDto {
  sessionId: string;
  closingBalance: number;
  notes?: string;
}

// Enum values for dropdowns
export const POS_SESSION_STATUS_OPTIONS = [
  { value: "Open", label: "Open" },
  { value: "Closed", label: "Closed" },
  { value: "Suspended", label: "Suspended" },
];

