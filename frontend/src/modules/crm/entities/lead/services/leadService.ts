import { APICore } from '../../../../../helpers/api/apiCore';
import type { LeadDto, CreateLeadDto, UpdateLeadDto } from '../types';

const api = new APICore();

class LeadService {
  /**
   * Get all leads
   */
  getAll = async (): Promise<LeadDto[]> => {
    const response = await api.get('/crm/leads', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get lead by ID
   */
  getById = async (id: string): Promise<LeadDto> => {
    const response = await api.get(`/crm/leads/${id}`, null);
    return response.data as LeadDto;
  };

  /**
   * Get leads by source
   */
  getBySource = async (source: string): Promise<LeadDto[]> => {
    const response = await api.get(`/crm/leads/source/${source}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get leads by status
   */
  getByStatus = async (status: string): Promise<LeadDto[]> => {
    const response = await api.get(`/crm/leads/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new lead
   */
  create = async (lead: CreateLeadDto): Promise<string> => {
    const response = await api.create('/crm/leads', lead);
    const data = response.data;
    if (typeof data === "string") return data;
    return data?.id || data?.leadId || data?.Id || "";
  };

  /**
   * Update lead
   */
  update = async (id: string, lead: UpdateLeadDto): Promise<void> => {
    await api.update(`/crm/leads/${id}`, { ...lead, id });
  };

  /**
   * Delete lead
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/leads/${id}`);
  };

  /**
   * Convert lead to customer
   */
  convertToCustomer = async (id: string): Promise<string> => {
    const response = await api.create(`/crm/leads/${id}/convert`, {});
    const data = response.data;
    if (typeof data === "string") return data;
    return data?.id || data?.customerId || data?.Id || "";
  };

  /**
   * Convert lead to opportunity
   */
  convertToOpportunity = async (id: string): Promise<string> => {
    const response = await api.create(`/crm/leads/${id}/convert-to-opportunity`, {});
    const data = response.data;
    if (typeof data === "string") return data;
    return data?.id || data?.opportunityId || data?.Id || "";
  };
}

const leadService = new LeadService();
export default leadService;

