import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface SystemLogDto {
  id: string;
  logLevel: string;
  message: string;
  exception?: string;
  userId?: string;
  userName?: string;
  timestamp: string;
  source?: string;
  category?: string;
}

class SystemLogService {
  getAll = async (): Promise<SystemLogDto[]> => {
    const response = await api.get('/admin/system-logs', {});
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getById = async (id: string): Promise<SystemLogDto> => {
    const response = await api.get(`/admin/system-logs/${id}`, null);
    return response.data as SystemLogDto;
  };

  getByLevel = async (logLevel: string): Promise<SystemLogDto[]> => {
    const response = await api.get(`/admin/system-logs/level/${logLevel}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getByUser = async (userId: string): Promise<SystemLogDto[]> => {
    const response = await api.get(`/admin/system-logs/user/${userId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  getByDateRange = async (startDate: Date, endDate: Date): Promise<SystemLogDto[]> => {
    const params = {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
    };
    const response = await api.get('/admin/system-logs/date-range', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };
}

export const systemLogService = new SystemLogService();

