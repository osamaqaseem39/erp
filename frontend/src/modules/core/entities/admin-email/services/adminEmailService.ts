import { APICore } from "../../../../../helpers/api/apiCore";

const api = new APICore();

export interface AdminEmailDto {
  id: string;
  email: string;
  type: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateAdminEmailDto {
  email: string;
  type: string;
}

export interface UpdateAdminEmailDto {
  email?: string;
  type?: string;
  isActive?: boolean;
}

export const ADMIN_EMAIL_TYPES = ["Notification", "Finance", "Support", "Sales", "Other"] as const;

class AdminEmailService {
  getAll = async (isActive?: boolean): Promise<AdminEmailDto[]> => {
    const params: Record<string, boolean> = {};
    if (isActive !== undefined) params.isActive = isActive;
    const response = await api.get("/admin/emails", params);
    return Array.isArray(response.data) ? response.data : [];
  };

  getById = async (id: string): Promise<AdminEmailDto> => {
    const response = await api.get(`/admin/emails/${id}`, null);
    return response.data as AdminEmailDto;
  };

  create = async (dto: CreateAdminEmailDto): Promise<AdminEmailDto> => {
    const response = await api.create("/admin/emails", dto);
    return response.data as AdminEmailDto;
  };

  update = async (id: string, dto: UpdateAdminEmailDto): Promise<AdminEmailDto> => {
    const response = await api.update(`/admin/emails/${id}`, dto);
    return response.data as AdminEmailDto;
  };

  delete = async (id: string): Promise<void> => {
    await api.delete(`/admin/emails/${id}`);
  };
}

export const adminEmailService = new AdminEmailService();
