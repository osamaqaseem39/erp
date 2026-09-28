import { APICore } from '../../../../../helpers/api/apiCore';
import { toPagedResult, toRequestParams, type PagedQuery, type PagedResult } from '../../../../../lib/erp/paging';
import type { EntityOption } from '../../../../../components/erp/EntitySelect';
import type { CustomerDto, CreateCustomerDto, UpdateCustomerDto } from '../types';

const api = new APICore();
const BASE = '/crm/customers';

export interface CustomerFilters {
  status: string;
  industry: string;
  customerType: string;
}

export type CustomerQuery = PagedQuery<CustomerFilters>;

/** Sort keys accepted by GET /crm/customers/paged (backend whitelist in CustomerRepository.SortFields). */
export const CUSTOMER_SORT_KEYS = ['customerName', 'customerCode', 'companyName', 'customerType', 'status', 'industry', 'email', 'creditLimit', 'createdAt'] as const;

class CustomerService {
  /** Server-side paged, filtered, sorted list. */
  list = async (query: CustomerQuery, signal?: AbortSignal): Promise<PagedResult<CustomerDto>> => {
    const response = await api.get(`${BASE}/paged`, toRequestParams(query), { signal });
    return toPagedResult<CustomerDto>(response.data);
  };

  get = async (id: string, signal?: AbortSignal): Promise<CustomerDto> => {
    const response = await api.get(`${BASE}/${id}`, null, { signal });
    return response.data as CustomerDto;
  };

  /** Returns the new customer's id. */
  create = async (customer: CreateCustomerDto): Promise<string> => {
    const response = await api.create(BASE, customer);
    return (response.data as { id: string }).id;
  };

  update = async (id: string, customer: UpdateCustomerDto): Promise<void> => {
    await api.update(`${BASE}/${id}`, { ...customer, id });
  };

  /** Refused with 409 when the customer has related records; the message says which. */
  remove = async (id: string): Promise<void> => {
    await api.delete(`${BASE}/${id}`);
  };

  /** Options for EntitySelect pickers on other forms. */
  searchOptions = async (searchTerm: string): Promise<EntityOption[]> => {
    const page = await this.list({ pageNumber: 1, pageSize: 20, searchTerm, sortBy: 'customerName', sortDirection: 'asc' });
    return page.items.map(toOption);
  };

  resolveOption = async (id: string): Promise<EntityOption | null> => {
    try {
      return toOption(await this.get(id));
    } catch {
      return null;
    }
  };

  /**
   * @deprecated Loads every customer. Kept for the dropdowns on other modules' forms until they
   * move to EntitySelect + searchOptions.
   */
  getAll = async (): Promise<CustomerDto[]> => {
    const response = await api.get(BASE, null);
    return Array.isArray(response.data) ? response.data : [];
  };
}

const toOption = (c: CustomerDto): EntityOption => ({ value: c.id ?? '', label: c.customerName, hint: c.customerCode });

export const customerService = new CustomerService();
