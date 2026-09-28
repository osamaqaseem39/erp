// Customer Entity Types
// Based on ERP.Shared.DTOs.CRM.CustomerDto

export interface CustomerDto {
  id?: string;
  tenantId?: string;
  companyId: string;
  customerCode: string;
  customerName: string;
  companyName: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  customerType: string; // "Individual" | "Company"
  status: string; // "Active" | "Inactive" | "Suspended"
  industry?: string;
  creditLimit: number;
  outstandingBalance: number;
  paymentTerms?: string;
  taxId?: string;
  website?: string;
  notes?: string;
  currencyCode: string;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCustomerDto {
  companyId: string;
  customerCode: string;
  customerName: string;
  companyName: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  customerType: string;
  status: string;
  industry?: string;
  creditLimit: number;
  outstandingBalance: number;
  paymentTerms?: string;
  taxId?: string;
  website?: string;
  notes?: string;
  currencyCode: string;
}

export interface UpdateCustomerDto {
  id: string;
  customerCode: string;
  customerName: string;
  companyName: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  customerType: string;
  status: string;
  industry?: string;
  creditLimit: number;
  outstandingBalance: number;
  paymentTerms?: string;
  taxId?: string;
  website?: string;
  notes?: string;
  currencyCode: string;
}

// Enum values for dropdowns
export const CUSTOMER_TYPE_OPTIONS = [
  { value: "Individual", label: "Individual" },
  { value: "Company", label: "Company" },
];

export const CUSTOMER_STATUS_OPTIONS = [
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
  { value: "Suspended", label: "Suspended" },
];

export const CURRENCY_OPTIONS = [
  { value: "USD", label: "USD - US Dollar" },
  { value: "EUR", label: "EUR - Euro" },
  { value: "GBP", label: "GBP - British Pound" },
  { value: "JPY", label: "JPY - Japanese Yen" },
  { value: "AUD", label: "AUD - Australian Dollar" },
  { value: "CAD", label: "CAD - Canadian Dollar" },
  { value: "CHF", label: "CHF - Swiss Franc" },
  { value: "CNY", label: "CNY - Chinese Yuan" },
];

export const PAYMENT_TERMS_OPTIONS = [
  { value: "", label: "Select payment terms" },
  { value: "Due on receipt", label: "Due on receipt" },
  { value: "Net 15", label: "Net 15" },
  { value: "Net 30", label: "Net 30" },
  { value: "Net 45", label: "Net 45" },
  { value: "Net 60", label: "Net 60" },
  { value: "Net 90", label: "Net 90" },
];

