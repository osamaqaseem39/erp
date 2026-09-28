import { APICore } from '../../../../../helpers/api/apiCore';
import type { TimeEntryDto, CreateTimeEntryDto, UpdateTimeEntryDto } from '../types';

const api = new APICore();

class TimeTrackingService {
  /**
   * Get all time entries
   */
  getAll = async (): Promise<TimeEntryDto[]> => {
    const response = await api.get('/hrm/time-tracking', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get time entry by ID
   */
  getById = async (id: string): Promise<TimeEntryDto> => {
    const response = await api.get(`/hrm/time-tracking/${id}`, null);
    return response.data as TimeEntryDto;
  };

  /**
   * Get time entries by employee
   */
  getByEmployee = async (employeeId: string): Promise<TimeEntryDto[]> => {
    const response = await api.get(`/hrm/time-tracking/employee/${employeeId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get time entries by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<TimeEntryDto[]> => {
    const response = await api.get(`/hrm/time-tracking/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get total hours for employee in date range
   */
  getTotalHours = async (employeeId: string, startDate: string, endDate: string): Promise<number> => {
    const response = await api.get(`/hrm/time-tracking/employee/${employeeId}/total-hours?startDate=${startDate}&endDate=${endDate}`, null);
    return response.data || 0;
  };

  /**
   * Create new time entry
   */
  create = async (timeEntry: CreateTimeEntryDto): Promise<string> => {
    const response = await api.create('/hrm/time-tracking', timeEntry);
    return response.data.id || response.data.timeEntryId;
  };

  /**
   * Update time entry
   */
  update = async (id: string, timeEntry: UpdateTimeEntryDto): Promise<void> => {
    await api.update(`/hrm/time-tracking/${id}`, { ...timeEntry, id });
  };

  /**
   * Delete time entry
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/time-tracking/${id}`);
  };
}

const timeTrackingService = new TimeTrackingService();
export default timeTrackingService;

