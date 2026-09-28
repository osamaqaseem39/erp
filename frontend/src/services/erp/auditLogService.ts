import { APICore } from "../../helpers/api/apiCore";
import type { AuditLogDto, CreateAuditLogDto } from "../../types/erp";

const api = new APICore();

export const auditLogService = {
  getAll: async (): Promise<AuditLogDto[]> => {
    const response = await api.get("/admin/audit-logs", null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getById: async (id: string): Promise<AuditLogDto> => {
    const response = await api.get(`/admin/audit-logs/${id}`, null);
    return response.data as AuditLogDto;
  },

  getByEntity: async (entityName: string, entityId: string): Promise<AuditLogDto[]> => {
    const response = await api.get(`/admin/audit-logs/entity/${entityName}/${entityId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getByUser: async (userId: string): Promise<AuditLogDto[]> => {
    const response = await api.get(`/admin/audit-logs/user/${userId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getByAction: async (action: string): Promise<AuditLogDto[]> => {
    const response = await api.get(`/admin/audit-logs/action/${action}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  getByDateRange: async (startDate: Date, endDate: Date): Promise<AuditLogDto[]> => {
    const params = {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
    };
    const response = await api.get("/admin/audit-logs/date-range", params);
    return Array.isArray(response.data) ? response.data : [response.data];
  },

  create: async (log: CreateAuditLogDto): Promise<string> => {
    const response = await api.create("/admin/audit-logs", log);
    return response.data.id;
  },
};

