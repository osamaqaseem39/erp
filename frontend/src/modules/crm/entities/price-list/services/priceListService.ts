import { APICore } from '../../../../../helpers/api/apiCore';
import type { PriceListDto, CreatePriceListDto, UpdatePriceListDto } from '../types';

const api = new APICore();

class PriceListService {
  getAll = async (): Promise<PriceListDto[]> => {
    const response = await api.get('/crm/price-lists', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<PriceListDto> => {
    const response = await api.get(`/crm/price-lists/${id}`, null);
    return response.data as PriceListDto;
  };

  create = async (priceList: CreatePriceListDto): Promise<string> => {
    const response = await api.create('/crm/price-lists', priceList);
    return response.data.id || response.data.priceListId;
  };

  update = async (id: string, priceList: UpdatePriceListDto): Promise<void> => {
    await api.update(`/crm/price-lists/${id}`, { ...priceList, id });
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/price-lists/${id}`);
  };
}

const priceListService = new PriceListService();
export default priceListService;
