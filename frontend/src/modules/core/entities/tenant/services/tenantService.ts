import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface TenantDto {
  id: string;
  name: string;
  tenantName?: string;
  domain: string;
  industry: string;
  country: string;
  status: string;
  databaseName: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateTenantDto {
  name: string;
  tenantName?: string;
  domain: string;
  industry: string;
  country: string;
  status: string;
  isActive: boolean;
}

export interface UpdateTenantDto {
  id: string;
  name?: string;
  tenantName?: string;
  domain?: string;
  industry?: string;
  country?: string;
  status?: string;
  isActive?: boolean;
}

class TenantService {
  getAll = async (): Promise<TenantDto[]> => {
    const response = await api.get('/tenants', {});
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /** Public endpoint for login page — no auth required. */
  getLoginOptions = async (): Promise<TenantDto[]> => {
    const response = await api.get('/tenants/login-options', {});
    const data = Array.isArray(response.data) ? response.data : [response.data];
    return data.map((t: any) => ({
      id: t.id ?? t.Id,
      name: t.name ?? t.Name ?? t.tenantName ?? t.TenantName ?? '',
      domain: t.domain ?? t.Domain ?? '',
      industry: '',
      country: '',
      status: 'Active',
      databaseName: '',
      isActive: true,
    }));
  };

  getById = async (id: string): Promise<TenantDto> => {
    const response = await api.get(`/tenants/${id}`, null);
    return response.data as TenantDto;
  };

  getByDomain = async (domain: string): Promise<TenantDto> => {
    const response = await api.get(`/tenants/domain/${domain}`, null);
    return response.data as TenantDto;
  };

  create = async (tenant: CreateTenantDto): Promise<TenantDto> => {
    const response = await api.create('/tenants', tenant);
    return response.data as TenantDto;
  };

  update = async (id: string, tenant: UpdateTenantDto): Promise<TenantDto> => {
    const response = await api.update(`/tenants/${id}`, { ...tenant, id });
    return response.data as TenantDto;
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/tenants/${id}`);
  };
}

export const tenantService = new TenantService();
