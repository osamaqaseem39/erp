// ItemCategory Entity Types
// Based on ERP.Shared.DTOs.Inventory.ItemCategoryDto

export interface ItemCategoryDto {
  id?: string;
  tenantId?: string;
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateItemCategoryDto {
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  isActive?: boolean;
}

export interface UpdateItemCategoryDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  isActive: boolean;
}

