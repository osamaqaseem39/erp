import { APICore } from '../../../../../helpers/api/apiCore';
import type { BankDto, CreateBankDto, UpdateBankDto } from '../types';

const api = new APICore();

class BankService {
  /**
   * Get all banks
   */
  getAll = async (): Promise<BankDto[]> => {
    const response = await api.get('/finance/banks', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get bank by ID
   */
  getById = async (id: string): Promise<BankDto> => {
    const response = await api.get(`/finance/banks/${id}`, null);
    return response.data as BankDto;
  };

  /**
   * Create new bank
   */
  create = async (bank: CreateBankDto): Promise<string> => {
    const response = await api.create('/finance/banks', bank);
    return response.data.id || response.data.bankId;
  };

  /**
   * Update bank
   */
  update = async (id: string, bank: UpdateBankDto): Promise<void> => {
    await api.update(`/finance/banks/${id}`, { ...bank, id });
  };

  /**
   * Delete bank
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/banks/${id}`);
  };
}

const bankService = new BankService();
export default bankService;

