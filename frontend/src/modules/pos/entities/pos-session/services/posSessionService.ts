import { APICore } from '../../../../../helpers/api/apiCore';
import type { POSSessionDto, CreatePOSSessionDto, UpdatePOSSessionDto, ClosePOSSessionDto } from '../types';

const api = new APICore();

class POSSessionService {
  /**
   * Get all POS sessions
   */
  getAll = async (cashierId?: string, status?: string): Promise<POSSessionDto[]> => {
    const params: any = {};
    if (cashierId) params.cashierId = cashierId;
    if (status) params.status = status;
    
    const response = await api.get('/sales/pos-sessions', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get POS session by ID
   */
  getById = async (id: string): Promise<POSSessionDto> => {
    const response = await api.get(`/sales/pos-sessions/${id}`, null);
    return response.data as POSSessionDto;
  };

  /**
   * Get POS sessions by cashier
   */
  getByCashier = async (cashierId: string): Promise<POSSessionDto[]> => {
    const response = await api.get('/sales/pos-sessions', { cashierId });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get POS sessions by status
   */
  getByStatus = async (status: string): Promise<POSSessionDto[]> => {
    const response = await api.get('/sales/pos-sessions', { status });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new POS session
   */
  create = async (session: CreatePOSSessionDto): Promise<string> => {
    const response = await api.create('/sales/pos-sessions', session);
    return response.data.SessionId || response.data.id || '';
  };

  /**
   * Update POS session
   */
  update = async (id: string, session: UpdatePOSSessionDto): Promise<boolean> => {
    const response = await api.update(`/sales/pos-sessions/${id}`, { ...session, id });
    return response.status === 200;
  };

  /**
   * Delete POS session
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/sales/pos-sessions/${id}`);
  };

  /**
   * Close POS session
   */
  closeSession = async (id: string, dto: ClosePOSSessionDto): Promise<boolean> => {
    const response = await api.create(`/sales/pos-sessions/${id}/close`, dto);
    return response.status === 200;
  };
}

export const posSessionService = new POSSessionService();

