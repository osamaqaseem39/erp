import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface SystemSettingDto {
  id: string;
  settingKey: string;
  settingValue: string;
  description?: string;
  settingType: string;
  category: string;
  isEncrypted: boolean;
  isReadOnly: boolean;
  validationRule?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSystemSettingDto {
  settingKey: string;
  settingValue: string;
  description?: string;
  settingType: string;
  category: string;
  isEncrypted: boolean;
  isReadOnly: boolean;
  validationRule?: string;
  notes?: string;
}

export interface UpdateSystemSettingDto {
  id: string;
  settingValue: string;
  description?: string;
  settingType?: string;
  isEncrypted?: boolean;
  isActive?: boolean;
  validationRule?: string;
  notes?: string;
}

export interface UpsertSystemSettingDto {
  settingKey: string;
  settingValue: string;
  description?: string;
  settingType: string;
  category: string;
  isEncrypted: boolean;
  isActive?: boolean;
  validationRule?: string;
  notes?: string;
}

class SystemSettingService {
  getAll = async (category?: string): Promise<SystemSettingDto[]> => {
    const params: any = {};
    if (category) params.category = category;
    
    const response = await api.get('/admin/system-settings', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<SystemSettingDto> => {
    const response = await api.get(`/admin/system-settings/${id}`, null);
    return response.data as SystemSettingDto;
  };

  getByKey = async (key: string): Promise<SystemSettingDto> => {
    const response = await api.get(`/admin/system-settings/key/${key}`, null);
    return response.data as SystemSettingDto;
  };

  getByCategory = async (category: string): Promise<SystemSettingDto[]> => {
    const response = await api.get(`/admin/system-settings/category/${category}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  create = async (setting: CreateSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.create('/admin/system-settings', setting);
    return response.data as SystemSettingDto;
  };

  update = async (id: string, setting: UpdateSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.update(`/admin/system-settings/${id}`, { ...setting, id });
    return response.data as SystemSettingDto;
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/admin/system-settings/${id}`);
  };

  upsert = async (setting: UpsertSystemSettingDto): Promise<SystemSettingDto> => {
    const response = await api.create('/admin/system-settings/upsert', setting);
    return response.data as SystemSettingDto;
  };
}

export const systemSettingService = new SystemSettingService();

