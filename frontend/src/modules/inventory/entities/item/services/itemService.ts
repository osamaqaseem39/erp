import { APICore } from '../../../../../helpers/api/apiCore';
import type { ItemDto, CreateItemDto, UpdateItemDto, ItemQuery } from '../types';

const api = new APICore();

class ItemService {
  /**
   * Get all items
   */
  getAll = async (query: ItemQuery): Promise<ItemDto[]> => {
    const params = new URLSearchParams();
    params.append('companyId', query.companyId);
    if (query.categoryId) params.append('categoryId', query.categoryId);
    
    const response = await api.get(`/inventory/items?${params.toString()}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get item by ID
   */
  getById = async (id: string): Promise<ItemDto> => {
    const response = await api.get(`/inventory/items/${id}`, null);
    return response.data as ItemDto;
  };

  /**
   * Get low stock items
   */
  getLowStockItems = async (companyId: string): Promise<ItemDto[]> => {
    const response = await api.get(`/inventory/items/low-stock?companyId=${companyId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new item
   */
  create = async (item: CreateItemDto): Promise<string> => {
    const response = await api.create('/inventory/items', item);
    return response.data.id || response.data.itemId;
  };

  /**
   * Update item
   */
  update = async (id: string, item: UpdateItemDto): Promise<void> => {
    await api.update(`/inventory/items/${id}`, { ...item, id });
  };

  /**
   * Delete item
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/items/${id}`);
  };
}

const itemService = new ItemService();
export default itemService;

