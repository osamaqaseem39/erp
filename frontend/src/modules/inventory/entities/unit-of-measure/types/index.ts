// UnitOfMeasure Entity Types
// Based on ERP.Shared.DTOs.Inventory.UnitOfMeasureDto

export interface UnitOfMeasureDto {
  id?: string;
  tenantId?: string;
  code: string;
  name: string;
  description?: string;
  symbol: string;
  conversionFactor: number;
  baseUnitId?: string;
  baseUnitName?: string;
  isBaseUnit: boolean;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateUnitOfMeasureDto {
  code: string;
  name: string;
  description?: string;
  symbol: string;
  conversionFactor: number;
  baseUnitId?: string;
  isBaseUnit: boolean;
  isActive?: boolean;
}

export interface UpdateUnitOfMeasureDto {
  id: string;
  code?: string;
  name?: string;
  description?: string;
  symbol?: string;
  conversionFactor?: number;
  baseUnitId?: string;
  isBaseUnit?: boolean;
  isActive?: boolean;
}

