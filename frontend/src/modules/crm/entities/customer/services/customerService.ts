import { APICore } from '../../../../../helpers/api/apiCore';
import type { CustomerDto, CreateCustomerDto, UpdateCustomerDto } from '../types';

const api = new APICore();

class CustomerService {
  /**
   * Get all customers
   */
  getAll = async (): Promise<CustomerDto[]> => {
    const response = await api.get('/crm/customers', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get customer by ID
   */
  getById = async (id: string): Promise<CustomerDto> => {
    const response = await api.get(`/crm/customers/${id}`, null);
    return response.data as CustomerDto;
  };

  /**
   * Search customers
   */
  search = async (searchTerm: string): Promise<CustomerDto[]> => {
    const response = await api.get('/crm/customers/search', { searchTerm });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get customers by status
   */
  getByStatus = async (status: string): Promise<CustomerDto[]> => {
    const response = await api.get(`/crm/customers/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get customers by industry
   */
  getByIndustry = async (industry: string): Promise<CustomerDto[]> => {
    const response = await api.get(`/crm/customers/industry/${industry}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new customer
   */
  create = async (customer: CreateCustomerDto): Promise<CustomerDto> => {
    const response = await api.create('/crm/customers', customer);
    return response.data as CustomerDto;
  };

  /**
   * Update customer
   */
  update = async (id: string, customer: UpdateCustomerDto): Promise<CustomerDto> => {
    const response = await api.update(`/crm/customers/${id}`, { ...customer, id });
    return response.data as CustomerDto;
  };

  /**
   * Delete customer
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/customers/${id}`);
  };
}

export const customerService = new CustomerService();

