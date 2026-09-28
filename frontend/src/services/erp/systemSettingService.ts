import { APICore } from "../../helpers/api/apiCore";
import type { SystemSettingDto, CreateSystemSettingDto, UpdateSystemSettingDto, UpsertSystemSettingDto } from "../../types/erp";

const api = new APICore();

export const systemSettingService = {
  getAll: async (category?: string): Promise<SystemSettingDto[]> => {
    const params: any = {};
    if (category) params.category = category;
    
    const response = await api.get("/admin/system-settings", params);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getById: async (id: string): Promise<SystemSettingDto> => {
    const response = await api.get(`/admin/system-settings/${id}`, null);
    return response.data as SystemSettingDto;
  },

  getByKey: async (key: string): Promise<SystemSettingDto> => {
    const response = await api.get(`/admin/system-settings/key/${key}`, null);
    return response.data as SystemSettingDto;
  },

  getByCategory: async (category: string): Promise<SystemSettingDto[]> => {
    const response = await api.get(`/admin/system-settings/category/${category}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  create: async (setting: CreateSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.create("/admin/system-settings", setting);
    return response.data as SystemSettingDto;
  },

  update: async (id: string, setting: UpdateSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.update(`/admin/system-settings/${id}`, { ...setting, id });
    return response.data as SystemSettingDto;
  },

  upsert: async (setting: UpsertSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.create("/admin/system-settings/upsert", setting);
    return response.data as SystemSettingDto;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/admin/system-settings/${id}`);
  },
};

