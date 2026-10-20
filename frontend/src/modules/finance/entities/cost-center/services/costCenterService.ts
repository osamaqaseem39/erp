import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface CostCenterDto {
  id?: string;
  costCenterId?: string;
  costCenterCode: string;
  costCenterName: string;
  description?: string;
  isActive?: boolean;
}

class CostCenterService {
  getAll = async (isActive?: boolean): Promise<CostCenterDto[]> => {
    const params = isActive === undefined ? '' : `?isActive=${isActive}`;
    const response = await api.get(`/finance/cost-centers${params}`, null);
    return Array.isArray(response.data) ? response.data : [];
  };

  create = async (dto: Partial<CostCenterDto>): Promise<string> => {
    const response = await api.create('/finance/cost-centers', dto);
    return response.data.id || response.data.costCenterId;
  };

  update = async (id: string, dto: Partial<CostCenterDto>): Promise<void> => {
    await api.update(`/finance/cost-centers/${id}`, dto);
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/cost-centers/${id}`);
  };
}

const costCenterService = new CostCenterService();
export default costCenterService;
