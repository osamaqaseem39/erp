import { APICore } from '../../../../../helpers/api/apiCore';
import type { POSTransactionDto, CreatePOSTransactionDto, UpdatePOSTransactionDto } from '../types';

const api = new APICore();

class POSTransactionService {
  /**
   * Get all POS transactions
   */
  getAll = async (sessionId?: string, customerId?: string, startDate?: string, endDate?: string): Promise<POSTransactionDto[]> => {
    const params: any = {};
    if (sessionId) params.sessionId = sessionId;
    if (customerId) params.customerId = customerId;
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    const response = await api.get('/sales/pos-transactions', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get POS transaction by ID
   */
  getById = async (id: string): Promise<POSTransactionDto> => {
    const response = await api.get(`/sales/pos-transactions/${id}`, null);
    return response.data as POSTransactionDto;
  };

  /**
   * Get POS transactions by session
   */
  getBySession = async (sessionId: string): Promise<POSTransactionDto[]> => {
    const response = await api.get('/sales/pos-transactions', { sessionId });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get POS transactions by customer
   */
  getByCustomer = async (customerId: string): Promise<POSTransactionDto[]> => {
    const response = await api.get('/sales/pos-transactions', { customerId });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get POS transactions by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<POSTransactionDto[]> => {
    const response = await api.get('/sales/pos-transactions', { startDate, endDate });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new POS transaction
   */
  create = async (transaction: CreatePOSTransactionDto): Promise<string> => {
    const response = await api.create('/sales/pos-transactions', transaction);
    return response.data.TransactionId || response.data.id || '';
  };

  /**
   * Update POS transaction
   */
  update = async (id: string, transaction: UpdatePOSTransactionDto): Promise<boolean> => {
    const response = await api.update(`/sales/pos-transactions/${id}`, { ...transaction, id });
    return response.status === 200;
  };

  /**
   * Delete POS transaction
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/sales/pos-transactions/${id}`);
  };
}

export const posTransactionService = new POSTransactionService();

