// Item Entity Types
// Based on ERP.Shared.DTOs.Inventory.ItemDto

export interface ItemDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  itemCode: string;
  itemName: string;
  description: string;
  itemCategoryId: string;
  itemCategoryName?: string;
  categoryId: string;
  categoryName?: string;
  itemType: string;
  unitOfMeasureId: string;
  unitOfMeasureName?: string;
  hsnCode: string;
  barCode: string;
  defaultWarehouseId?: string;
  defaultWarehouseName?: string;
  costPrice: number;
  sellingPrice: number;
  minimumStock: number;
  maximumStock: number;
  reorderLevel: number;
  reorderQuantity: number;
  isSerialized: boolean;
  isBatchTracked: boolean;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateItemDto {
  companyId: string;
  itemCode: string;
  itemName: string;
  description: string;
  itemCategoryId: string;
  categoryId: string;
  itemType: string;
  unitOfMeasureId: string;
  hsnCode: string;
  barCode: string;
  defaultWarehouseId?: string;
  costPrice: number;
  sellingPrice: number;
  minimumStock: number;
  maximumStock: number;
  reorderLevel: number;
  reorderQuantity: number;
  isSerialized: boolean;
  isBatchTracked: boolean;
  isActive?: boolean;
}

export interface UpdateItemDto {
  id: string;
  itemCode: string;
  itemName: string;
  description: string;
  itemCategoryId: string;
  categoryId: string;
  itemType: string;
  unitOfMeasureId: string;
  hsnCode: string;
  barCode: string;
  defaultWarehouseId?: string;
  costPrice: number;
  sellingPrice: number;
  minimumStock: number;
  maximumStock: number;
  reorderLevel: number;
  reorderQuantity: number;
  isSerialized: boolean;
  isBatchTracked: boolean;
  isActive: boolean;
}

export interface ItemQuery {
  companyId: string;
  categoryId?: string;
}

// Item Type Options
export const ITEM_TYPE_OPTIONS = [
  { value: "Raw", label: "Raw Material" },
  { value: "Finished", label: "Finished Good" },
  { value: "Service", label: "Service" },
  { value: "Consumable", label: "Consumable" },
];

