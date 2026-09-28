// StockAdjustment Entity Types
// Based on ERP.Shared.DTOs.Inventory.StockAdjustmentDto

export interface StockAdjustmentDetailDto {
  id: string;
  stockAdjustmentId: string;
  itemId: string;
  itemName?: string;
  quantityOnHand: number;
  quantityCounted: number;
  quantityDifference: number;
  notes?: string;
}

export interface StockAdjustmentDto {
  id?: string;
  tenantId?: string;
  adjustmentNumber: string;
  adjustmentDate: string;
  warehouseId: string;
  warehouseName?: string;
  adjustmentType: string;
  status: string;
  reference?: string;
  notes?: string;
  details?: StockAdjustmentDetailDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateStockAdjustmentDetailDto {
  itemId: string;
  quantityOnHand: number;
  quantityCounted: number;
  notes?: string;
}

export interface CreateStockAdjustmentDto {
  adjustmentNumber?: string;
  adjustmentDate: string;
  warehouseId: string;
  adjustmentType?: string;
  reference?: string;
  notes?: string;
  details?: CreateStockAdjustmentDetailDto[];
}

export interface UpdateStockAdjustmentDetailDto {
  id?: string;
  itemId?: string;
  quantityOnHand?: number;
  quantityCounted?: number;
  notes?: string;
}

export interface UpdateStockAdjustmentDto {
  id: string;
  adjustmentNumber?: string;
  adjustmentDate?: string;
  warehouseId?: string;
  adjustmentType?: string;
  status?: string;
  reference?: string;
  notes?: string;
  details?: UpdateStockAdjustmentDetailDto[];
}

// Stock Adjustment Type Options
export const STOCK_ADJUSTMENT_TYPE_OPTIONS = [
  { value: "Physical", label: "Physical Count" },
  { value: "Damaged", label: "Damaged" },
  { value: "Expired", label: "Expired" },
  { value: "Lost", label: "Lost" },
  { value: "Found", label: "Found" },
  { value: "Other", label: "Other" },
];

// Stock Adjustment Status Options
export const STOCK_ADJUSTMENT_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Approved", label: "Approved" },
  { value: "Posted", label: "Posted" },
  { value: "Cancelled", label: "Cancelled" },
];

