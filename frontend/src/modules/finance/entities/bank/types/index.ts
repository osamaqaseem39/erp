// Bank Entity Types
// Based on ERP.Shared.DTOs.Finance.BankDto

export interface BankDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  bankName: string;
  bankCode: string;
  swiftCode: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phone: string;
  email: string;
  website: string;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateBankDto {
  companyId: string;
  bankName: string;
  bankCode: string;
  swiftCode: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  phone: string;
  email: string;
  website: string;
  isActive?: boolean;
}

export interface UpdateBankDto {
  id: string;
  bankName?: string;
  bankCode?: string;
  swiftCode?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  website?: string;
  isActive?: boolean;
}

