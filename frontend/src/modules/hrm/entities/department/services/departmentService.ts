import { APICore } from '../../../../../helpers/api/apiCore';
import type { DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto } from '../types';

const api = new APICore();

class DepartmentService {
  /**
   * Get all departments
   */
  getAll = async (companyId?: string): Promise<DepartmentDto[]> => {
    const params: any = {};
    if (companyId) params.companyId = companyId;
    
    const response = await api.get('/hrm/departments', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get department by ID
   */
  getById = async (id: string): Promise<DepartmentDto> => {
    const response = await api.get(`/hrm/departments/${id}`, null);
    return response.data as DepartmentDto;
  };

  /**
   * Create new department
   */
  create = async (department: CreateDepartmentDto): Promise<DepartmentDto> => {
    const response = await api.create('/hrm/departments', department);
    return response.data as DepartmentDto;
  };

  /**
   * Update department
   */
  update = async (id: string, department: UpdateDepartmentDto): Promise<DepartmentDto> => {
    const response = await api.update(`/hrm/departments/${id}`, { ...department, id });
    return response.data as DepartmentDto;
  };

  /**
   * Delete department
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/departments/${id}`);
  };
}

export const departmentService = new DepartmentService();

