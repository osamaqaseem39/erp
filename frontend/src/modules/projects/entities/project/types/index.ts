// Project Entity Types
// Based on ERP.Shared.DTOs.Projects.ProjectDto

export interface ProjectDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  projectCode: string;
  projectName: string;
  description?: string;
  clientId?: string;
  clientName?: string;
  projectManagerId?: string;
  projectManagerName?: string;
  startDate?: string;
  endDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  status: string;
  priority: string;
  budget: number;
  actualCost: number;
  progress: number;
  notes?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateProjectDto {
  companyId?: string;
  projectCode: string;
  projectName: string;
  description?: string;
  clientId?: string;
  projectManagerId?: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  priority?: string;
  budget?: number;
  notes?: string;
}

export interface UpdateProjectDto {
  id: string;
  projectCode?: string;
  projectName?: string;
  description?: string;
  clientId?: string;
  projectManagerId?: string;
  startDate?: string;
  endDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  status?: string;
  priority?: string;
  budget?: number;
  actualCost?: number;
  progress?: number;
  notes?: string;
}

// Project Status Options
export const PROJECT_STATUS_OPTIONS = [
  { value: "Planning", label: "Planning" },
  { value: "InProgress", label: "In Progress" },
  { value: "OnHold", label: "On Hold" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

// Project Priority Options
export const PROJECT_PRIORITY_OPTIONS = [
  { value: "Low", label: "Low" },
  { value: "Medium", label: "Medium" },
  { value: "High", label: "High" },
  { value: "Critical", label: "Critical" },
];

