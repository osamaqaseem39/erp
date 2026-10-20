import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

export interface AccountDefaultDto {
  accountDefaultId?: string;
  purpose: string;
  accountId: string;
  accountCode?: string;
  accountName?: string;
  isActive?: boolean;
}

export const ACCOUNT_PURPOSES = [
  "Cash", "BankClearing", "AccountsReceivable", "AccountsPayable",
  "SalesRevenue", "SalesReturns", "CostOfGoodsSold", "Inventory",
  "InputTax", "OutputTax", "RetainedEarnings", "ExchangeGain", "ExchangeLoss",
];

class AccountDefaultService {
  getAll = async (): Promise<AccountDefaultDto[]> => {
    const response = await api.get('/finance/account-defaults', null);
    return Array.isArray(response.data) ? response.data : [];
  };

  upsert = async (defaults: { purpose: string; accountId: string; isActive?: boolean }[]): Promise<AccountDefaultDto[]> => {
    const response = await api.update('/finance/account-defaults', { defaults });
    return Array.isArray(response.data) ? response.data : [];
  };
}

const accountDefaultService = new AccountDefaultService();
export default accountDefaultService;
