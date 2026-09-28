import { APICore } from '../../../../../helpers/api/apiCore';
import type { CustomerGroupDto, CreateCustomerGroupDto, UpdateCustomerGroupDto } from '../types';

const api = new APICore();

class CustomerGroupService {
  getAll = async (): Promise<CustomerGroupDto[]> => {
    const response = await api.get('/crm/customer-groups', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<CustomerGroupDto> => {
    const response = await api.get(`/crm/customer-groups/${id}`, null);
    return response.data as CustomerGroupDto;
  };

  create = async (customerGroup: CreateCustomerGroupDto): Promise<string> => {
    const response = await api.create('/crm/customer-groups', customerGroup);
    return response.data.id || response.data.customerGroupId;
  };

  update = async (id: string, customerGroup: UpdateCustomerGroupDto): Promise<void> => {
    await api.update(`/crm/customer-groups/${id}`, { ...customerGroup, id });
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/customer-groups/${id}`);
  };
}

const customerGroupService = new CustomerGroupService();
export default customerGroupService;
