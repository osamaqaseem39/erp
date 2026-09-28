// Campaign Entity Types
// Based on ERP.Shared.DTOs.CRM.CampaignDto

export interface CampaignDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  campaignName?: string;
  campaignType?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  actualCost?: number;
  expectedResponse?: number;
  actualResponse?: number;
  description?: string;
  currency?: string;
  targetAudience?: string;
  goals?: string;
  launchDate?: string;
  pausedDate?: string;
  completedDate?: string;
  isActive?: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCampaignDto {
  companyId?: string;
  campaignName?: string;
  campaignType?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  actualCost?: number;
  expectedResponse?: number;
  actualResponse?: number;
  description?: string;
  currency?: string;
  targetAudience?: string;
  goals?: string;
}

export interface UpdateCampaignDto {
  id: string;
  companyId?: string;
  campaignName?: string;
  campaignType?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  actualCost?: number;
  expectedResponse?: number;
  actualResponse?: number;
  description?: string;
  currency?: string;
  targetAudience?: string;
  goals?: string;
}

// Campaign Type Options
export const CAMPAIGN_TYPE_OPTIONS = [
  { value: "Email", label: "Email" },
  { value: "Social Media", label: "Social Media" },
  { value: "Print", label: "Print" },
  { value: "Web", label: "Web" },
  { value: "Event", label: "Event" },
  { value: "Telemarketing", label: "Telemarketing" },
  { value: "Other", label: "Other" },
];

// Campaign Status Options
export const CAMPAIGN_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Scheduled", label: "Scheduled" },
  { value: "Active", label: "Active" },
  { value: "Paused", label: "Paused" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

