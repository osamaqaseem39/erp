import { APICore } from '../../../../../helpers/api/apiCore';
import type { StockAdjustmentDto, CreateStockAdjustmentDto, UpdateStockAdjustmentDto } from '../types';

const api = new APICore();

class StockAdjustmentService {
  /**
   * Get all stock adjustments
   */
  getAll = async (): Promise<StockAdjustmentDto[]> => {
    const response = await api.get('/inventory/adjustments', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get stock adjustment by ID
   */
  getById = async (id: string): Promise<StockAdjustmentDto> => {
    const response = await api.get(`/inventory/adjustments/${id}`, null);
    return response.data as StockAdjustmentDto;
  };

  /**
   * Create new stock adjustment
   */
  create = async (adjustment: CreateStockAdjustmentDto): Promise<string> => {
    const response = await api.create('/inventory/adjustments', adjustment);
    return response.data.id || response.data.adjustmentId;
  };

  /**
   * Update stock adjustment
   */
  update = async (id: string, adjustment: UpdateStockAdjustmentDto): Promise<void> => {
    await api.update(`/inventory/adjustments/${id}`, { ...adjustment, id });
  };

  /**
   * Delete stock adjustment
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/adjustments/${id}`);
  };
}

const stockAdjustmentService = new StockAdjustmentService();
export default stockAdjustmentService;

