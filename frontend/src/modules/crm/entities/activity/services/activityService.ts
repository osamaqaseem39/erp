import { APICore } from '../../../../../helpers/api/apiCore';
import type { ActivityDto, CreateActivityDto, UpdateActivityDto } from '../types';

const api = new APICore();

class ActivityService {
  /**
   * Get all activities
   */
  getAll = async (): Promise<ActivityDto[]> => {
    const response = await api.get('/crm/activities', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activity by ID
   */
  getById = async (id: string): Promise<ActivityDto> => {
    const response = await api.get(`/crm/activities/${id}`, null);
    return response.data as ActivityDto;
  };

  /**
   * Get activities by customer
   */
  getByCustomer = async (customerId: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/customer/${customerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activities by lead
   */
  getByLead = async (leadId: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/lead/${leadId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activities by opportunity
   */
  getByOpportunity = async (opportunityId: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/opportunity/${opportunityId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activities by assigned user
   */
  getByAssignedTo = async (assignedTo: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/assigned/${assignedTo}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activities by type
   */
  getByType = async (activityType: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/type/${activityType}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get activities by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<ActivityDto[]> => {
    const response = await api.get(`/crm/activities/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new activity
   */
  create = async (activity: CreateActivityDto): Promise<string> => {
    const response = await api.create('/crm/activities', activity);
    return response.data.id || response.data.activityId;
  };

  /**
   * Update activity
   */
  update = async (id: string, activity: UpdateActivityDto): Promise<void> => {
    await api.update(`/crm/activities/${id}`, { ...activity, id });
  };

  /**
   * Delete activity
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/activities/${id}`);
  };

  complete = async (id: string): Promise<void> => {
    await api.create(`/crm/activities/${id}/complete`, {});
  };

  reschedule = async (id: string, newDate: string): Promise<void> => {
    await api.create(`/crm/activities/${id}/reschedule`, { newDate });
  };
}

const activityService = new ActivityService();
export default activityService;

