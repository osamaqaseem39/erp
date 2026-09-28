import { APICore } from '../../../../../helpers/api/apiCore';
import type { PayrollDto, CreatePayrollDto, UpdatePayrollDto } from '../types';

const api = new APICore();

class PayrollService {
  /**
   * Get all payrolls
   */
  getAll = async (): Promise<PayrollDto[]> => {
    const response = await api.get('/hrm/payroll', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get payroll by ID
   */
  getById = async (id: string): Promise<PayrollDto> => {
    const response = await api.get(`/hrm/payroll/${id}`, null);
    return response.data as PayrollDto;
  };

  /**
   * Get payrolls by employee
   */
  getByEmployee = async (employeeId: string): Promise<PayrollDto[]> => {
    const response = await api.get(`/hrm/payroll/employee/${employeeId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get payrolls by period
   */
  getByPeriod = async (startDate: string, endDate: string): Promise<PayrollDto[]> => {
    const response = await api.get(`/hrm/payroll/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new payroll
   */
  create = async (payroll: CreatePayrollDto): Promise<string> => {
    const response = await api.create('/hrm/payroll', payroll);
    return response.data.id || response.data.payrollId;
  };

  /**
   * Update payroll
   */
  update = async (id: string, payroll: UpdatePayrollDto): Promise<void> => {
    await api.update(`/hrm/payroll/${id}`, { ...payroll, id });
  };

  /**
   * Delete payroll
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/payroll/${id}`);
  };

  /**
   * Process payroll
   */
  process = async (id: string): Promise<void> => {
    await api.create(`/hrm/payroll/${id}/process`, {});
  };
}

const payrollService = new PayrollService();
export default payrollService;

