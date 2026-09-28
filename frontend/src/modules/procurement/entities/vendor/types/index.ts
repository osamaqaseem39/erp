// Vendor Entity Types
// Based on ERP.Shared.DTOs.Procurement.VendorDto

export interface VendorBankAccountDto {
  id?: string;
  vendorId?: string;
  bankName?: string;
  accountNumber?: string;
  accountHolderName?: string;
  iban?: string;
  swiftCode?: string;
  branchName?: string;
  isDefault?: boolean;
}

export interface VendorDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  companyName?: string;
  vendorCode: string;
  vendorName: string;
  vendorType?: string;
  phone?: string;
  address?: string;
  status?: string;
  bankAccounts?: VendorBankAccountDto[];
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  taxNumber?: string;
  website?: string;
  paymentTerms?: string;
  creditLimit?: number;
  category?: string;
  notes?: string;
  isApproved: boolean;
  approvedAt?: string;
  approvedBy?: string;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateVendorDto {
  vendorCode: string;
  vendorName: string;
  vendorType?: string;
  phone?: string;
  address?: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  taxNumber?: string;
  website?: string;
  paymentTerms?: string;
  creditLimit?: number;
  status?: string;
  category?: string;
  notes?: string;
  companyId: string;
  isActive?: boolean;
}

export interface UpdateVendorDto {
  id: string;
  vendorCode?: string;
  vendorName?: string;
  vendorType?: string;
  phone?: string;
  address?: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  taxNumber?: string;
  website?: string;
  paymentTerms?: string;
  creditLimit?: number;
  status?: string;
  category?: string;
  notes?: string;
  isActive?: boolean;
  vendorId?: string;
}

// Vendor Status Options
export const VENDOR_STATUS_OPTIONS = [
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
  { value: "Suspended", label: "Suspended" },
  { value: "Pending", label: "Pending Approval" },
];

// Vendor Type Options
export const VENDOR_TYPE_OPTIONS = [
  { value: "Supplier", label: "Supplier" },
  { value: "Manufacturer", label: "Manufacturer" },
  { value: "Distributor", label: "Distributor" },
  { value: "Service Provider", label: "Service Provider" },
  { value: "Other", label: "Other" },
];

