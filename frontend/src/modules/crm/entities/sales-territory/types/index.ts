// Sales Territory Entity Types
// Based on ERP.Shared.DTOs.CRM.SalesTerritoryDto

export interface SalesTerritoryDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  territoryName?: string;
  description?: string;
  managerId?: string;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSalesTerritoryDto {
  companyId?: string;
  territoryName?: string;
  description?: string;
  managerId?: string;
}

export interface UpdateSalesTerritoryDto {
  id: string;
  territoryName?: string;
  description?: string;
  managerId?: string;
}
