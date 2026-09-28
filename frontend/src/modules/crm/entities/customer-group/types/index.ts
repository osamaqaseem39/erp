// Customer Group Entity Types
// Based on ERP.Shared.DTOs.CRM.CustomerGroupDto

export interface CustomerGroupDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  groupName?: string;
  description?: string;
  discountPercentage?: number;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCustomerGroupDto {
  companyId?: string;
  groupName?: string;
  description?: string;
  discountPercentage?: number;
}

export interface UpdateCustomerGroupDto {
  id: string;
  groupName?: string;
  description?: string;
  discountPercentage?: number;
}
