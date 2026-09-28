import { APICore } from '../../../../../helpers/api/apiCore';
import type { CurrencyDto, CreateCurrencyDto, UpdateCurrencyDto } from '../types';

const api = new APICore();

class CurrencyService {
  /**
   * Get all currencies
   */
  getAll = async (): Promise<CurrencyDto[]> => {
    const response = await api.get('/finance/currencies', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get currency by code
   */
  getById = async (currencyCode: string): Promise<CurrencyDto> => {
    const response = await api.get(`/finance/currencies/${currencyCode}`, null);
    return response.data as CurrencyDto;
  };

  /**
   * Get active currencies
   */
  getActive = async (): Promise<CurrencyDto[]> => {
    const response = await api.get('/finance/currencies/active', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new currency
   */
  create = async (currency: CreateCurrencyDto): Promise<void> => {
    await api.create('/finance/currencies', currency);
  };

  /**
   * Update currency
   */
  update = async (currencyCode: string, currency: UpdateCurrencyDto): Promise<void> => {
    await api.update(`/finance/currencies/${currencyCode}`, currency);
  };

  /**
   * Delete currency
   */
  delete = async (currencyCode: string): Promise<void> => {
    await api.delete(`/finance/currencies/${currencyCode}`);
  };
}

const currencyService = new CurrencyService();
export default currencyService;

