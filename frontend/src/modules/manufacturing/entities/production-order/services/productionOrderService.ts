import { APICore } from '../../../../../helpers/api/apiCore';
import type { ProductionOrderDto, CreateProductionOrderDto, UpdateProductionOrderDto } from '../types';

const api = new APICore();

class ProductionOrderService {
  /**
   * Get all production orders
   */
  getAll = async (status?: string, workCenterId?: string): Promise<ProductionOrderDto[]> => {
    const params: any = {};
    if (status) params.status = status;
    if (workCenterId) params.workCenterId = workCenterId;

    const response = await api.get('/manufacturing/production-orders', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get production order by ID
   */
  getById = async (id: string): Promise<ProductionOrderDto> => {
    const response = await api.get(`/manufacturing/production-orders/${id}`, null);
    return response.data as ProductionOrderDto;
  };

  /**
   * Create new production order
   */
  create = async (productionOrder: CreateProductionOrderDto): Promise<string> => {
    const response = await api.create('/manufacturing/production-orders', productionOrder);
    return response.data.Id || response.data.id || response.data;
  };

  /**
   * Update production order
   */
  update = async (id: string, productionOrder: UpdateProductionOrderDto): Promise<void> => {
    await api.update(`/manufacturing/production-orders/${id}`, { ...productionOrder, id });
  };

  /**
   * Delete production order
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/manufacturing/production-orders/${id}`);
  };

  /**
   * Start production order
   */
  start = async (id: string): Promise<void> => {
    await api.create(`/manufacturing/production-orders/${id}/start`, {});
  };

  /**
   * Complete production order
   */
  complete = async (id: string): Promise<void> => {
    await api.create(`/manufacturing/production-orders/${id}/complete`, {});
  };

  /**
   * Cancel production order
   */
  cancel = async (id: string, reason: string): Promise<void> => {
    await api.create(`/manufacturing/production-orders/${id}/cancel`, { reason });
  };
}

const productionOrderService = new ProductionOrderService();
export default productionOrderService;

