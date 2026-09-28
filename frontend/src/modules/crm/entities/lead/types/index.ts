// Lead Entity Types
// Based on ERP.Shared.DTOs.CRM.LeadDto

export interface LeadDto {
  id?: string;
  tenantId?: string;
  leadCode: string;
  firstName: string;
  lastName: string;
  companyName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  leadSource: string;
  source: string;
  status: string;
  priority: string;
  estimatedValue?: number;
  lastContactDate?: string;
  assignedToUserId?: string;
  assignedToId?: string;
  companyId?: string;
  contactPerson?: string;
  postalCode?: string;
  industry?: string;
  assignedTo?: string;
  leadScore?: number;
  notes?: string;
  convertedDate?: string;
  isActive: boolean;
  assignedToUserName?: string;
  assignedToName?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateLeadDto {
  leadCode: string;
  firstName: string;
  lastName: string;
  companyName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  leadSource?: string;
  source?: string;
  status?: string;
  priority?: string;
  estimatedValue?: number;
  lastContactDate?: string;
  assignedToUserId?: string;
  assignedToId?: string;
  companyId?: string;
  contactPerson?: string;
  postalCode?: string;
  industry?: string;
  assignedTo?: string;
  leadScore?: number;
  notes?: string;
}

export interface UpdateLeadDto {
  id: string;
  leadCode: string;
  firstName: string;
  lastName: string;
  companyName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  leadSource?: string;
  source?: string;
  status?: string;
  priority?: string;
  estimatedValue?: number;
  lastContactDate?: string;
  assignedToUserId?: string;
  assignedToId?: string;
  contactPerson?: string;
  postalCode?: string;
  industry?: string;
  assignedTo?: string;
  leadScore?: number;
  notes?: string;
}

// Lead Source Options
export const LEAD_SOURCE_OPTIONS = [
  { value: "Employee Referral", label: "Employee Referral" },
  { value: "External Referral", label: "External Referral" },
  { value: "Website", label: "Website" },
  { value: "Referral", label: "Referral" },
  { value: "Social Media", label: "Social Media" },
  { value: "Email Campaign", label: "Email Campaign" },
  { value: "Trade Show", label: "Trade Show" },
  { value: "Cold Call", label: "Cold Call" },
  { value: "Other", label: "Other" },
];

// Lead Status Options
export const LEAD_STATUS_OPTIONS = [
  { value: "New", label: "New" },
  { value: "Contacted", label: "Contacted" },
  { value: "Qualified", label: "Qualified" },
  { value: "Converted", label: "Converted" },
  { value: "Lost", label: "Lost" },
  { value: "Nurturing", label: "Nurturing" },
];

// Lead Priority Options
export const LEAD_PRIORITY_OPTIONS = [
  { value: "Low", label: "Low" },
  { value: "Medium", label: "Medium" },
  { value: "High", label: "High" },
  { value: "Urgent", label: "Urgent" },
];

