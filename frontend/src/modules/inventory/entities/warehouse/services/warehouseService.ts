import { APICore } from '../../../../../helpers/api/apiCore';
import type { WarehouseDto, CreateWarehouseDto, UpdateWarehouseDto } from '../types';

const api = new APICore();

class WarehouseService {
  /**
   * Get all warehouses
   */
  getAll = async (isActive?: boolean): Promise<WarehouseDto[]> => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    
    const queryString = params.toString();
    const url = queryString ? `/inventory/warehouses?${queryString}` : '/inventory/warehouses';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get warehouse by ID
   */
  getById = async (id: string): Promise<WarehouseDto> => {
    const response = await api.get(`/inventory/warehouses/${id}`, null);
    return response.data as WarehouseDto;
  };

  /**
   * Get active warehouses
   */
  getActive = async (): Promise<WarehouseDto[]> => {
    const response = await api.get('/inventory/warehouses?isActive=true', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new warehouse
   */
  create = async (warehouse: CreateWarehouseDto): Promise<string> => {
    const response = await api.create('/inventory/warehouses', warehouse);
    return response.data.id || response.data.warehouseId;
  };

  /**
   * Update warehouse
   */
  update = async (id: string, warehouse: UpdateWarehouseDto): Promise<void> => {
    await api.update(`/inventory/warehouses/${id}`, { ...warehouse, id });
  };

  /**
   * Delete warehouse
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/warehouses/${id}`);
  };

  /**
   * Activate warehouse
   */
  activate = async (id: string): Promise<void> => {
    await api.create(`/inventory/warehouses/${id}/activate`, {});
  };

  /**
   * Deactivate warehouse
   */
  deactivate = async (id: string): Promise<void> => {
    await api.create(`/inventory/warehouses/${id}/deactivate`, {});
  };
}

const warehouseService = new WarehouseService();
export default warehouseService;

