import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  resource?: string;
  action?: string;
}

export interface CreateRoleDto {
  name: string;
  description?: string;
  isActive: boolean;
}

export interface UpdateRoleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface AssignPermissionsDto {
  roleId: string;
  permissionIds: string[];
}

class RoleService {
  getAll = async (isActive?: boolean): Promise<RoleDto[]> => {
    const params: any = {};
    if (isActive !== undefined) params.isActive = isActive;
    
    const response = await api.get('/admin/roles', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<RoleDto> => {
    const response = await api.get(`/admin/roles/${id}`, null);
    return response.data as RoleDto;
  };

  create = async (role: CreateRoleDto): Promise<RoleDto> => {
    const response = await api.create('/admin/roles', role);
    return response.data as RoleDto;
  };

  update = async (id: string, role: UpdateRoleDto): Promise<RoleDto> => {
    const response = await api.update(`/admin/roles/${id}`, { ...role, id });
    return response.data as RoleDto;
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/admin/roles/${id}`);
  };

  getPermissions = async (id: string): Promise<PermissionDto[]> => {
    const response = await api.get(`/admin/roles/${id}/permissions`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  assignPermissions = async (id: string, dto: AssignPermissionsDto): Promise<RoleDto> => {
    const response = await api.create(`/admin/roles/${id}/permissions`, dto);
    return response.data as RoleDto;
  };

  getAllPermissions = async (): Promise<PermissionDto[]> => {
    const response = await api.get("/admin/permissions", { activeOnly: true });
    return Array.isArray(response.data) ? response.data : [response.data];
  };
}

export const roleService = new RoleService();

