// StockTransfer Entity Types
// Based on ERP.Shared.DTOs.Inventory.StockTransferDto

export interface StockTransferDetailDto {
  id: string;
  stockTransferId: string;
  itemId: string;
  itemName?: string;
  quantity: number;
  notes?: string;
}

export interface StockTransferDto {
  id?: string;
  tenantId?: string;
  transferNumber: string;
  transferDate: string;
  fromWarehouseId: string;
  fromWarehouseName?: string;
  toWarehouseId: string;
  toWarehouseName?: string;
  status: string;
  reference?: string;
  notes?: string;
  details?: StockTransferDetailDto[];
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateStockTransferDetailDto {
  itemId: string;
  quantity: number;
  notes?: string;
}

export interface CreateStockTransferDto {
  transferNumber?: string;
  transferDate: string;
  fromWarehouseId: string;
  toWarehouseId: string;
  reference?: string;
  notes?: string;
  details?: CreateStockTransferDetailDto[];
}

export interface UpdateStockTransferDetailDto {
  id?: string;
  itemId?: string;
  quantity?: number;
  notes?: string;
}

export interface UpdateStockTransferDto {
  id: string;
  transferNumber?: string;
  transferDate?: string;
  fromWarehouseId?: string;
  toWarehouseId?: string;
  status?: string;
  reference?: string;
  notes?: string;
  details?: UpdateStockTransferDetailDto[];
}

// Stock Transfer Status Options
export const STOCK_TRANSFER_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Approved", label: "Approved" },
  { value: "InTransit", label: "In Transit" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

