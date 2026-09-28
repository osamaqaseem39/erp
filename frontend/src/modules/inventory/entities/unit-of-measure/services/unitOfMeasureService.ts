import { APICore } from '../../../../../helpers/api/apiCore';
import type { UnitOfMeasureDto, CreateUnitOfMeasureDto, UpdateUnitOfMeasureDto } from '../types';

const api = new APICore();

class UnitOfMeasureService {
  /**
   * Get all units of measure
   */
  getAll = async (isActive?: boolean): Promise<UnitOfMeasureDto[]> => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    
    const queryString = params.toString();
    const url = queryString ? `/inventory/units-of-measure?${queryString}` : '/inventory/units-of-measure';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get unit of measure by ID
   */
  getById = async (id: string): Promise<UnitOfMeasureDto> => {
    const response = await api.get(`/inventory/units-of-measure/${id}`, null);
    return response.data as UnitOfMeasureDto;
  };

  /**
   * Create new unit of measure
   */
  create = async (unit: CreateUnitOfMeasureDto): Promise<string> => {
    const response = await api.create('/inventory/units-of-measure', unit);
    return response.data.id || response.data.unitId;
  };

  /**
   * Update unit of measure
   */
  update = async (id: string, unit: UpdateUnitOfMeasureDto): Promise<void> => {
    await api.update(`/inventory/units-of-measure/${id}`, { ...unit, id });
  };

  /**
   * Delete unit of measure
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/units-of-measure/${id}`);
  };
}

const unitOfMeasureService = new UnitOfMeasureService();
export default unitOfMeasureService;

