import { APICore } from '../../../../../helpers/api/apiCore';
import type { EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto, EmployeeListDto, EmployeeQuery } from '../types';

const api = new APICore();

class EmployeeService {
  /**
   * Get all employees with optional filtering
   */
  getAll = async (query?: EmployeeQuery): Promise<EmployeeListDto[]> => {
    const params: any = {};
    if (query?.searchTerm) params.searchTerm = query.searchTerm;
    if (query?.departmentId) params.departmentId = query.departmentId;
    if (query?.status) params.status = query.status;
    if (query?.pageNumber) params.pageNumber = query.pageNumber;
    if (query?.pageSize) params.pageSize = query.pageSize;
    if (query?.companyId) params.companyId = query.companyId;
    if (query?.tenantId) params.tenantId = query.tenantId;
    if (query?.page) params.page = query.page;
    
    const response = await api.get('/hrm/employees/getAllEmployees', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get employee by ID
   */
  getById = async (id: string): Promise<EmployeeDto> => {
    const response = await api.get('/hrm/employees/getEmployeeById', { employeeId: id });
    return response.data as EmployeeDto;
  };

  /**
   * Get employee by employee code
   */
  getByCode = async (employeeCode: string): Promise<EmployeeDto> => {
    const response = await api.get('/hrm/employees/getEmployeeByCode', { employeeCode });
    return response.data as EmployeeDto;
  };

  /**
   * Check if employee code is unique
   */
  isCodeUnique = async (employeeCode: string, excludeEmployeeId?: string): Promise<boolean> => {
    const params: any = { employeeCode };
    if (excludeEmployeeId) params.excludeEmployeeId = excludeEmployeeId;
    const response = await api.get('/hrm/employees/isEmployeeCodeUnique', params);
    return response.data.isUnique || false;
  };

  /**
   * Create new employee
   */
  create = async (employee: CreateEmployeeDto): Promise<string> => {
    const response = await api.create('/hrm/employees/addEmployee', employee);
    return response.data.id || response.data.employeeId || '';
  };

  /**
   * Update employee
   */
  update = async (employee: UpdateEmployeeDto): Promise<boolean> => {
    const response = await api.update('/hrm/employees/updateEmployee', employee);
    return response.data.success !== false;
  };

  /**
   * Delete employee
   */
  delete = async (employeeId: string): Promise<void> => {
    await api.delete(`/hrm/employees/deleteEmployee?employeeId=${employeeId}`);
  };
}

export const employeeService = new EmployeeService();

