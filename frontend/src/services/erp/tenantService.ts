import { APICore } from "../../helpers/api/apiCore";
import type { Tenant, CreateTenantDto, UpdateTenantDto } from "../../types/erp";

const api = new APICore();

export const tenantService = {
  getAll: async (): Promise<Tenant[]> => {
    try {
      const response = await api.get("/tenants", null);
      if (!response || !response.data) {
        return [];
      }
      return Array.isArray(response.data) ? response.data : [response.data];
    } catch (error) {
      console.error("Error fetching tenants:", error);
      return [];
    }
  },

  getById: async (id: string): Promise<Tenant> => {
    const response = await api.get(`/tenants/${id}`, null);
    if (!response || !response.data) {
      throw new Error("Failed to fetch tenant");
    }
    return response.data as Tenant;
  },

  getByDomain: async (domain: string): Promise<Tenant> => {
    const response = await api.get(`/tenants/domain/${domain}`, null);
    if (!response || !response.data) {
      throw new Error("Failed to fetch tenant by domain");
    }
    return response.data as Tenant;
  },

  create: async (tenant: CreateTenantDto): Promise<Tenant> => {
    const response = await api.create("/tenants", tenant);
    if (!response || !response.data) {
      throw new Error("Failed to create tenant");
    }
    return response.data as Tenant;
  },

  update: async (id: string, tenant: UpdateTenantDto): Promise<Tenant> => {
    const response = await api.update(`/tenants/${id}`, { ...tenant, id });
    if (!response || !response.data) {
      throw new Error("Failed to update tenant");
    }
    return response.data as Tenant;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/tenants/${id}`);
  },
};

