// Training Entity Types
// Based on ERP.Shared.DTOs.HRM.TrainingDto

export interface TrainingDto {
  id?: string;
  tenantId?: string;
  title: string;
  description: string;
  trainingType: string;
  startDate: string;
  endDate: string;
  duration: number;
  status: string;
  instructorId?: string;
  instructorName?: string;
  instructor?: string;
  location: string;
  cost: number;
  maxParticipants: number;
  notes?: string;
  isActive: boolean;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateTrainingDto {
  title: string;
  description: string;
  trainingType: string;
  startDate: string;
  endDate: string;
  duration: number;
  status?: string;
  instructorId?: string;
  instructor?: string;
  location: string;
  cost?: number;
  maxParticipants: number;
  notes?: string;
  isActive?: boolean;
}

export interface UpdateTrainingDto {
  id: string;
  title?: string;
  description?: string;
  trainingType?: string;
  startDate?: string;
  endDate?: string;
  duration?: number;
  status?: string;
  instructorId?: string;
  instructor?: string;
  location?: string;
  cost?: number;
  maxParticipants?: number;
  notes?: string;
  isActive?: boolean;
}

// Training Type Options
export const TRAINING_TYPE_OPTIONS = [
  { value: "Onsite", label: "Onsite" },
  { value: "Online", label: "Online" },
  { value: "Workshop", label: "Workshop" },
  { value: "Seminar", label: "Seminar" },
  { value: "Conference", label: "Conference" },
  { value: "Other", label: "Other" },
];

// Training Status Options
export const TRAINING_STATUS_OPTIONS = [
  { value: "Scheduled", label: "Scheduled" },
  { value: "InProgress", label: "In Progress" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "Postponed", label: "Postponed" },
];

