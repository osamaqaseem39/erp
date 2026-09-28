import { APICore } from '../../../../../helpers/api/apiCore';
import type { ItemCategoryDto, CreateItemCategoryDto, UpdateItemCategoryDto } from '../types';

const api = new APICore();

class ItemCategoryService {
  /**
   * Get all item categories
   */
  getAll = async (): Promise<ItemCategoryDto[]> => {
    const response = await api.get('/inventory/categories', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get item category by ID
   */
  getById = async (id: string): Promise<ItemCategoryDto> => {
    const response = await api.get(`/inventory/categories/${id}`, null);
    return response.data as ItemCategoryDto;
  };

  /**
   * Create new item category
   */
  create = async (category: CreateItemCategoryDto): Promise<string> => {
    const response = await api.create('/inventory/categories', category);
    return response.data.id || response.data.categoryId;
  };

  /**
   * Update item category
   */
  update = async (id: string, category: UpdateItemCategoryDto): Promise<void> => {
    await api.update(`/inventory/categories/${id}`, { ...category, id });
  };

  /**
   * Delete item category
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/categories/${id}`);
  };
}

const itemCategoryService = new ItemCategoryService();
export default itemCategoryService;

