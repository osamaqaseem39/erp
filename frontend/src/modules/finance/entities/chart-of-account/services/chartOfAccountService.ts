import { APICore } from '../../../../../helpers/api/apiCore';
import type { ChartOfAccountDto, CreateChartOfAccountDto, UpdateChartOfAccountDto, ChartOfAccountQuery } from '../types';

const api = new APICore();

class ChartOfAccountService {
  /**
   * Get all chart of accounts
   */
  getAll = async (query?: ChartOfAccountQuery): Promise<ChartOfAccountDto[]> => {
    const params = new URLSearchParams();
    if (query?.accountType) params.append('accountType', query.accountType);
    if (query?.isActive !== undefined) params.append('isActive', query.isActive.toString());
    
    const queryString = params.toString();
    const url = queryString ? `/finance/accounting/chart-of-accounts?${queryString}` : '/finance/accounting/chart-of-accounts';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get chart of account by ID
   */
  getById = async (id: string): Promise<ChartOfAccountDto> => {
    const response = await api.get(`/finance/accounting/chart-of-accounts/${id}`, null);
    return response.data as ChartOfAccountDto;
  };

  /**
   * Create new chart of account
   */
  create = async (account: CreateChartOfAccountDto): Promise<string> => {
    const response = await api.create('/finance/accounting/chart-of-accounts', account);
    return response.data.accountId || response.data.id;
  };

  /**
   * Update chart of account
   */
  update = async (id: string, account: UpdateChartOfAccountDto): Promise<void> => {
    await api.update(`/finance/accounting/chart-of-accounts/${id}`, account);
  };

  /**
   * Delete chart of account
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/accounting/chart-of-accounts/${id}`);
  };
}

const chartOfAccountService = new ChartOfAccountService();
export default chartOfAccountService;

