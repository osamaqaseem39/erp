// Opportunity Entity Types
// Based on ERP.Shared.DTOs.CRM.OpportunityDto

export interface OpportunityDto {
  id?: string;
  tenantId?: string;
  opportunityCode?: string;
  opportunityName?: string;
  title?: string;
  customerId?: string;
  leadId?: string;
  stage?: string;
  status?: string;
  estimatedValue?: number;
  actualValue?: number;
  amount?: number;
  currency?: string;
  probability?: number;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  convertedDate?: string;
  description?: string;
  notes?: string;
  source?: string;
  assignedTo?: string;
  assignedToUserId?: string;
  isActive?: boolean;
  customerName?: string;
  leadName?: string;
  assignedToName?: string;
  assignedToUserName?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateOpportunityDto {
  opportunityCode?: string;
  opportunityName?: string;
  title?: string;
  customerId?: string;
  leadId?: string;
  stage?: string;
  status?: string;
  estimatedValue?: number;
  actualValue?: number;
  amount?: number;
  currency?: string;
  probability?: number;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  description?: string;
  notes?: string;
  source?: string;
  assignedTo?: string;
  assignedToUserId?: string;
}

export interface UpdateOpportunityDto {
  id: string;
  opportunityCode?: string;
  opportunityName?: string;
  title?: string;
  customerId?: string;
  leadId?: string;
  stage?: string;
  status?: string;
  estimatedValue?: number;
  actualValue?: number;
  amount?: number;
  currency?: string;
  probability?: number;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  description?: string;
  notes?: string;
  source?: string;
  assignedTo?: string;
  assignedToUserId?: string;
}

// Opportunity Stage Options
export const OPPORTUNITY_STAGE_OPTIONS = [
  { value: "Prospecting", label: "Prospecting" },
  { value: "Qualification", label: "Qualification" },
  { value: "Needs Analysis", label: "Needs Analysis" },
  { value: "Value Proposition", label: "Value Proposition" },
  { value: "Id. Decision Makers", label: "Id. Decision Makers" },
  { value: "Perception Analysis", label: "Perception Analysis" },
  { value: "Proposal/Price Quote", label: "Proposal/Price Quote" },
  { value: "Negotiation/Review", label: "Negotiation/Review" },
  { value: "Closed Won", label: "Closed Won" },
  { value: "Closed Lost", label: "Closed Lost" },
];

// Opportunity Status Options
export const OPPORTUNITY_STATUS_OPTIONS = [
  { value: "Open", label: "Open" },
  { value: "Won", label: "Won" },
  { value: "Lost", label: "Lost" },
  { value: "On Hold", label: "On Hold" },
  { value: "Cancelled", label: "Cancelled" },
];

