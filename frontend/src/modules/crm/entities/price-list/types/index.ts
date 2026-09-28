// Price List Entity Types
// Based on ERP.Shared.DTOs.CRM.PriceListDto

export interface PriceListDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  priceListName?: string;
  description?: string;
  validFrom?: string;
  validTo?: string;
  isDefault?: boolean;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePriceListDto {
  companyId?: string;
  priceListName?: string;
  description?: string;
  validFrom?: string;
  validTo?: string;
  isDefault?: boolean;
}

export interface UpdatePriceListDto {
  id: string;
  priceListName?: string;
  description?: string;
  validFrom?: string;
  validTo?: string;
  isDefault?: boolean;
}
