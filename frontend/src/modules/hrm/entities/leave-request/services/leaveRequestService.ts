import { APICore } from '../../../../../helpers/api/apiCore';
import type { LeaveRequestDto, CreateLeaveRequestDto, UpdateLeaveRequestDto } from '../types';

const api = new APICore();

class LeaveRequestService {
  /**
   * Get all leave requests
   */
  getAll = async (): Promise<LeaveRequestDto[]> => {
    const response = await api.get('/hrm/leaves', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get leave request by ID
   */
  getById = async (id: string): Promise<LeaveRequestDto> => {
    const response = await api.get(`/hrm/leaves/${id}`, null);
    return response.data as LeaveRequestDto;
  };

  /**
   * Get leave requests by employee
   */
  getByEmployee = async (employeeId: string): Promise<LeaveRequestDto[]> => {
    const response = await api.get(`/hrm/leaves/employee/${employeeId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get leave requests by status
   */
  getByStatus = async (status: string): Promise<LeaveRequestDto[]> => {
    const response = await api.get(`/hrm/leaves/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new leave request
   */
  create = async (leaveRequest: CreateLeaveRequestDto): Promise<string> => {
    const response = await api.create('/hrm/leaves', leaveRequest);
    return response.data.id || response.data.leaveRequestId;
  };

  /**
   * Update leave request
   */
  update = async (id: string, leaveRequest: UpdateLeaveRequestDto): Promise<void> => {
    await api.update(`/hrm/leaves/${id}`, { ...leaveRequest, id });
  };

  /**
   * Delete leave request
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/leaves/${id}`);
  };

  /**
   * Approve leave request
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/hrm/leaves/${id}/approve`, {});
  };

  /**
   * Reject leave request
   */
  reject = async (id: string, reason?: string): Promise<void> => {
    await api.create(`/hrm/leaves/${id}/reject`, { reason });
  };
}

const leaveRequestService = new LeaveRequestService();
export default leaveRequestService;

