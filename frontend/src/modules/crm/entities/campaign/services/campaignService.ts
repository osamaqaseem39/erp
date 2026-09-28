import { APICore } from '../../../../../helpers/api/apiCore';
import type { CampaignDto, CreateCampaignDto, UpdateCampaignDto } from '../types';

const api = new APICore();

class CampaignService {
  /**
   * Get all campaigns
   */
  getAll = async (): Promise<CampaignDto[]> => {
    const response = await api.get('/crm/campaigns', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get campaign by ID
   */
  getById = async (id: string): Promise<CampaignDto> => {
    const response = await api.get(`/crm/campaigns/${id}`, null);
    return response.data as CampaignDto;
  };

  /**
   * Get campaigns by status
   */
  getByStatus = async (status: string): Promise<CampaignDto[]> => {
    const response = await api.get(`/crm/campaigns/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get campaigns by type
   */
  getByType = async (campaignType: string): Promise<CampaignDto[]> => {
    const response = await api.get(`/crm/campaigns/type/${campaignType}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get campaigns by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<CampaignDto[]> => {
    const response = await api.get(`/crm/campaigns/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new campaign
   */
  create = async (campaign: CreateCampaignDto): Promise<string> => {
    const response = await api.create('/crm/campaigns', campaign);
    return response.data.id || response.data.campaignId;
  };

  /**
   * Update campaign
   */
  update = async (id: string, campaign: UpdateCampaignDto): Promise<void> => {
    await api.update(`/crm/campaigns/${id}`, { ...campaign, id });
  };

  /**
   * Delete campaign
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/campaigns/${id}`);
  };

  /**
   * Launch campaign
   */
  launch = async (id: string): Promise<void> => {
    await api.create(`/crm/campaigns/${id}/launch`, {});
  };

  /**
   * Pause campaign
   */
  pause = async (id: string): Promise<void> => {
    await api.create(`/crm/campaigns/${id}/pause`, {});
  };

  /**
   * Complete campaign
   */
  complete = async (id: string): Promise<void> => {
    await api.create(`/crm/campaigns/${id}/complete`, {});
  };
}

const campaignService = new CampaignService();
export default campaignService;

