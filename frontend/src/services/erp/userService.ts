import { APICore } from "../../helpers/api/apiCore";
import type { UserDto, CreateUserDto, UpdateUserDto, ResetUserPasswordDto } from "../../types/erp";

const api = new APICore();

export const userService = {
  getAll: async (role?: string, isActive?: boolean, searchTerm?: string): Promise<UserDto[]> => {
    const params: any = {};
    if (role) params.role = role;
    if (isActive !== undefined) params.isActive = isActive;
    if (searchTerm) params.searchTerm = searchTerm;
    
    const response = await api.get("/admin/users", params);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getById: async (id: string): Promise<UserDto> => {
    const response = await api.get(`/admin/users/${id}`, null);
    return response.data as UserDto;
  },

  create: async (user: CreateUserDto, tenantId?: string): Promise<string> => {
    // Set tenantId in header if provided (for registration without auth)
    const headers: any = {};
    if (tenantId) {
      headers["X-Tenant-Id"] = tenantId;
    }
    
    const response = await api.create("/admin/users", user, Object.keys(headers).length > 0 ? headers : undefined);
    return response.data.userId || response.data.id;
  },

  update: async (id: string, user: UpdateUserDto): Promise<boolean> => {
    const response = await api.update(`/admin/users/${id}`, { ...user, id });
    return response.data.success || true;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/admin/users/${id}`);
  },

  resetPassword: async (id: string, dto: ResetUserPasswordDto): Promise<boolean> => {
    const response = await api.create(`/admin/users/${id}/reset-password`, dto);
    return response.data.success || true;
  },

  activate: async (id: string): Promise<void> => {
    await api.create(`/admin/users/${id}/activate`, {});
  },

  deactivate: async (id: string): Promise<void> => {
    await api.create(`/admin/users/${id}/deactivate`, {});
  },
};

