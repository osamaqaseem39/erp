import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateUserDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
  isActive: boolean;
  roleIds: string[];
}

export interface UpdateUserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  isActive: boolean;
  roleIds: string[];
}

export interface ResetUserPasswordDto {
  userId: string;
  newPassword: string;
}

class UserService {
  getAll = async (role?: string, isActive?: boolean, searchTerm?: string): Promise<UserDto[]> => {
    const params: any = {};
    if (role) params.role = role;
    if (isActive !== undefined) params.isActive = isActive;
    if (searchTerm) params.searchTerm = searchTerm;
    
    const response = await api.get('/admin/users', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<UserDto> => {
    const response = await api.get(`/admin/users/${id}`, null);
    return response.data as UserDto;
  };

  create = async (user: CreateUserDto, tenantId?: string): Promise<string> => {
    const headers = tenantId ? { 'X-Tenant-Id': tenantId } : undefined;
    const response = await api.create('/admin/users', user, headers);
    return response.data.userId || response.data.id;
  };

  update = async (id: string, user: UpdateUserDto): Promise<boolean> => {
    const response = await api.update(`/admin/users/${id}`, { ...user, id });
    return response.data.success || true;
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/admin/users/${id}`);
  };

  activate = async (id: string): Promise<void> => {
    await api.create(`/admin/users/${id}/activate`, {});
  };

  deactivate = async (id: string): Promise<void> => {
    await api.create(`/admin/users/${id}/deactivate`, {});
  };

  resetPassword = async (id: string, dto: ResetUserPasswordDto): Promise<boolean> => {
    const response = await api.create(`/admin/users/${id}/reset-password`, dto);
    return response.data.success || true;
  };
}

export const userService = new UserService();

