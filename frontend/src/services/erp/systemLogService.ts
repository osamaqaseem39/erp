import { APICore } from "../../helpers/api/apiCore";
import type { SystemLogDto, CreateSystemLogDto, UpdateSystemLogDto } from "../../types/erp";

const api = new APICore();

export const systemLogService = {
  getAll: async (): Promise<SystemLogDto[]> => {
    const response = await api.get("/admin/system-logs", null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getById: async (id: string): Promise<SystemLogDto> => {
    const response = await api.get(`/admin/system-logs/${id}`, null);
    return response.data as SystemLogDto;
  },

  getByLevel: async (level: string): Promise<SystemLogDto[]> => {
    const response = await api.get(`/admin/system-logs/level/${level}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getByUser: async (userId: string): Promise<SystemLogDto[]> => {
    const response = await api.get(`/admin/system-logs/user/${userId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getByDateRange: async (startDate: Date, endDate: Date): Promise<SystemLogDto[]> => {
    const params = {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
    };
    const response = await api.get("/admin/system-logs/date-range", params);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  create: async (log: CreateSystemLogDto): Promise<string> => {
    const response = await api.create("/admin/system-logs", log);
    return response.data.id;
  },

  update: async (id: string, log: UpdateSystemLogDto): Promise<void> => {
    await api.update(`/admin/system-logs/${id}`, { ...log, id });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/admin/system-logs/${id}`);
  },
};

