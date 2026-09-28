// LeaveRequest Entity Types
// Based on ERP.Shared.DTOs.HRM.LeaveRequestDto

export interface LeaveRequestDto {
  id?: string;
  tenantId?: string;
  companyId?: string;
  employeeId: string;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  daysRequested: number;
  reason?: string;
  status: string;
  notes?: string;
  appliedAt: string;
  approvedBy?: string;
  approvedAt?: string;
  rejectedBy?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  createdBy?: string;
  updatedBy?: string;
  employeeName?: string;
  leaveTypeName?: string;
  approvedByUserName?: string;
}

export interface CreateLeaveRequestDto {
  tenantId?: string;
  companyId?: string;
  employeeId: string;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  daysRequested: number;
  reason?: string;
  status?: string;
  notes?: string;
  appliedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  rejectedBy?: string;
  rejectedAt?: string;
  rejectionReason?: string;
}

export interface UpdateLeaveRequestDto {
  id: string;
  tenantId?: string;
  companyId?: string;
  employeeId?: string;
  leaveTypeId?: string;
  startDate?: string;
  endDate?: string;
  daysRequested?: number;
  reason?: string;
  status?: string;
  notes?: string;
  appliedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  rejectedBy?: string;
  rejectedAt?: string;
  rejectionReason?: string;
}

// Leave Request Status Options
export const LEAVE_REQUEST_STATUS_OPTIONS = [
  { value: "Pending", label: "Pending" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
  { value: "Cancelled", label: "Cancelled" },
];

