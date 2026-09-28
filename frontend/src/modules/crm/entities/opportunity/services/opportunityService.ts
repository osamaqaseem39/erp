import { APICore } from '../../../../../helpers/api/apiCore';
import type { OpportunityDto, CreateOpportunityDto, UpdateOpportunityDto } from '../types';

const api = new APICore();

class OpportunityService {
  /**
   * Get all opportunities
   */
  getAll = async (): Promise<OpportunityDto[]> => {
    const response = await api.get('/crm/opportunities', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get opportunity by ID
   */
  getById = async (id: string): Promise<OpportunityDto> => {
    const response = await api.get(`/crm/opportunities/${id}`, null);
    return response.data as OpportunityDto;
  };

  /**
   * Get opportunities by customer
   */
  getByCustomer = async (customerId: string): Promise<OpportunityDto[]> => {
    const response = await api.get(`/crm/opportunities/customer/${customerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get opportunities by status
   */
  getByStatus = async (status: string): Promise<OpportunityDto[]> => {
    const response = await api.get(`/crm/opportunities/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get opportunities by stage
   */
  getByStage = async (stage: string): Promise<OpportunityDto[]> => {
    const response = await api.get(`/crm/opportunities/stage/${stage}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get opportunities by assigned user
   */
  getByAssignedTo = async (assignedTo: string): Promise<OpportunityDto[]> => {
    const response = await api.get(`/crm/opportunities/assigned/${assignedTo}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new opportunity
   */
  create = async (opportunity: CreateOpportunityDto): Promise<string> => {
    const response = await api.create('/crm/opportunities', opportunity);
    return response.data.id || response.data.opportunityId;
  };

  /**
   * Update opportunity
   */
  update = async (id: string, opportunity: UpdateOpportunityDto): Promise<void> => {
    await api.update(`/crm/opportunities/${id}`, { ...opportunity, id });
  };

  /**
   * Delete opportunity
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/opportunities/${id}`);
  };

  /**
   * Convert opportunity to quote
   */
  convertToQuote = async (id: string): Promise<void> => {
    await api.create(`/crm/opportunities/${id}/convert`, {});
  };
}

const opportunityService = new OpportunityService();
export default opportunityService;

