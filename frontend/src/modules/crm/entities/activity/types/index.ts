// Activity Entity Types
// Based on ERP.Shared.DTOs.CRM.ActivityDto

export interface ActivityDto {
  id?: string;
  tenantId?: string;
  type?: string;
  activityType: string;
  subject: string;
  description?: string;
  startDate: string;
  endDate?: string;
  status: string;
  priority: string;
  assignedToUserId: string;
  assignedToUserName?: string;
  assignedTo?: string;
  location?: string;
  notes?: string;
  relatedEntityId?: string;
  relatedToId?: string;
  relatedEntityType?: string;
  relatedToType?: string;
  customerId?: string;
  customerName?: string;
  leadId?: string;
  leadName?: string;
  opportunityId?: string;
  opportunityName?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateActivityDto {
  activityType: string;
  subject: string;
  description?: string;
  startDate: string;
  endDate?: string;
  status?: string;
  priority?: string;
  assignedToUserId: string;
  location?: string;
  notes?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  customerId?: string;
  leadId?: string;
  opportunityId?: string;
}

export interface UpdateActivityDto {
  id: string;
  activityType: string;
  subject: string;
  description?: string;
  startDate: string;
  endDate?: string;
  status?: string;
  priority?: string;
  assignedToUserId: string;
  location?: string;
  notes?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  customerId?: string;
  leadId?: string;
  opportunityId?: string;
}

// Activity Type Options
export const ACTIVITY_TYPE_OPTIONS = [
  { value: "Call", label: "Call" },
  { value: "Email", label: "Email" },
  { value: "Meeting", label: "Meeting" },
  { value: "Task", label: "Task" },
  { value: "Note", label: "Note" },
  { value: "Other", label: "Other" },
];

// Activity Status Options
export const ACTIVITY_STATUS_OPTIONS = [
  { value: "Open", label: "Open" },
  { value: "InProgress", label: "In Progress" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Deferred", label: "Deferred" },
];

// Activity Priority Options
export const ACTIVITY_PRIORITY_OPTIONS = [
  { value: "Low", label: "Low" },
  { value: "Medium", label: "Medium" },
  { value: "High", label: "High" },
  { value: "Urgent", label: "Urgent" },
];

// Related Entity Type Options
export const RELATED_ENTITY_TYPE_OPTIONS = [
  { value: "Customer", label: "Customer" },
  { value: "Lead", label: "Lead" },
  { value: "Opportunity", label: "Opportunity" },
  { value: "Quote", label: "Quote" },
  { value: "SalesOrder", label: "Sales Order" },
];

