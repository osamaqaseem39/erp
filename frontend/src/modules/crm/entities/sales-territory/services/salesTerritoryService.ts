import { APICore } from '../../../../../helpers/api/apiCore';
import type { SalesTerritoryDto, CreateSalesTerritoryDto, UpdateSalesTerritoryDto } from '../types';

const api = new APICore();

class SalesTerritoryService {
  getAll = async (): Promise<SalesTerritoryDto[]> => {
    const response = await api.get('/crm/sales-territories', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<SalesTerritoryDto> => {
    const response = await api.get(`/crm/sales-territories/${id}`, null);
    return response.data as SalesTerritoryDto;
  };

  create = async (salesTerritory: CreateSalesTerritoryDto): Promise<string> => {
    const response = await api.create('/crm/sales-territories', salesTerritory);
    return response.data.id || response.data.salesTerritoryId;
  };

  update = async (id: string, salesTerritory: UpdateSalesTerritoryDto): Promise<void> => {
    await api.update(`/crm/sales-territories/${id}`, { ...salesTerritory, id });
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/sales-territories/${id}`);
  };
}

const salesTerritoryService = new SalesTerritoryService();
export default salesTerritoryService;
