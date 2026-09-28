// Contact Entity Types
// Based on ERP.Shared.DTOs.CRM.ContactDto

export interface ContactDto {
  id?: string;
  tenantId?: string;
  customerId: string;
  customerName?: string;
  firstName: string;
  lastName: string;
  fullName: string;
  jobTitle?: string;
  department?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  isPrimary: boolean;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateContactDto {
  customerId: string;
  firstName: string;
  lastName: string;
  jobTitle?: string;
  department?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  isPrimary?: boolean;
  notes?: string;
}

export interface UpdateContactDto {
  id: string;
  firstName?: string;
  lastName?: string;
  jobTitle?: string;
  department?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  isPrimary?: boolean;
  notes?: string;
}

