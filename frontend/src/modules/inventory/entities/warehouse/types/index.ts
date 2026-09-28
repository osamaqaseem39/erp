// Warehouse Entity Types
// Based on ERP.Shared.DTOs.Inventory.WarehouseDto

export interface WarehouseDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  warehouseCode: string;
  warehouseName: string;
  description?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phoneNumber?: string;
  email?: string;
  managerId?: string;
  contactPerson?: string;
  isDefault: boolean;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateWarehouseDto {
  companyId: string;
  warehouseCode: string;
  warehouseName: string;
  description?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phoneNumber?: string;
  email?: string;
  managerId?: string;
  contactPerson?: string;
  isDefault?: boolean;
  isActive?: boolean;
}

export interface UpdateWarehouseDto {
  id: string;
  warehouseCode?: string;
  warehouseName?: string;
  description?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  phoneNumber?: string;
  email?: string;
  managerId?: string;
  contactPerson?: string;
  isDefault?: boolean;
  isActive?: boolean;
}

