import { APICore } from '../../../../../helpers/api/apiCore';
import type { ReturnOrderDto, CreateReturnOrderDto, UpdateReturnOrderDto } from '../types';

const api = new APICore();

class ReturnOrderService {
  getAll = async (): Promise<ReturnOrderDto[]> => {
    const response = await api.get('/crm/return-orders', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<ReturnOrderDto> => {
    const response = await api.get(`/crm/return-orders/${id}`, null);
    return response.data as ReturnOrderDto;
  };

  create = async (returnOrder: CreateReturnOrderDto): Promise<string> => {
    const response = await api.create('/crm/return-orders', returnOrder);
    return response.data.id || response.data.returnOrderId;
  };

  update = async (id: string, returnOrder: UpdateReturnOrderDto): Promise<void> => {
    await api.update(`/crm/return-orders/${id}`, { ...returnOrder, id });
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/return-orders/${id}`);
  };
}

const returnOrderService = new ReturnOrderService();
export default returnOrderService;
