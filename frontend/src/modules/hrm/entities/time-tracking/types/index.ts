// TimeTracking (TimeEntry) Entity Types
// Based on ERP.Shared.DTOs.HRM.TimeEntryDto

export interface TimeEntryDto {
  id?: string;
  tenantId?: string;
  employeeId: string;
  employeeName?: string;
  projectId?: string;
  projectName?: string;
  taskId?: string;
  taskName?: string;
  startTime: string;
  endTime: string;
  workDate: string;
  hours: number;
  hoursWorked: number;
  description?: string;
  status: string;
  isBillable: boolean;
  isOvertime: boolean;
  hourlyRate?: number;
  totalAmount?: number;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
  isActive?: boolean;
}

export interface CreateTimeEntryDto {
  employeeId: string;
  projectId?: string;
  taskId?: string;
  startTime?: string;
  endTime?: string;
  workDate?: string;
  hours?: number;
  hoursWorked?: number;
  description?: string;
  status?: string;
  isBillable?: boolean;
  hourlyRate?: number;
  notes?: string;
}

export interface UpdateTimeEntryDto {
  id: string;
  employeeId?: string;
  projectId?: string;
  taskId?: string;
  startTime?: string;
  endTime?: string;
  workDate?: string;
  hours?: number;
  hoursWorked?: number;
  description?: string;
  status?: string;
  isBillable?: boolean;
  hourlyRate?: number;
  notes?: string;
}

// Time Entry Status Options
export const TIME_ENTRY_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Submitted", label: "Submitted" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
  { value: "Billed", label: "Billed" },
];

