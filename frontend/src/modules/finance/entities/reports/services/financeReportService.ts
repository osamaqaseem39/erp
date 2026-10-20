import { APICore } from '../../../../../helpers/api/apiCore';

const api = new APICore();

function qs(params: Record<string, string | number | boolean | undefined | null>) {
  const p = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') p.append(k, String(v));
  });
  const s = p.toString();
  return s ? `?${s}` : '';
}

class FinanceReportService {
  trialBalance = async (fromDate: string, toDate: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/trial-balance${qs({ fromDate, toDate, currencyCode })}`, null);
    return response.data;
  };

  runningTrialBalance = async (fromDate: string, toDate: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/running-trial-balance${qs({ fromDate, toDate, currencyCode })}`, null);
    return response.data;
  };

  profitLoss = async (fromDate: string, toDate: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/profit-loss${qs({ fromDate, toDate, currencyCode })}`, null);
    return response.data;
  };

  balanceSheet = async (asOfDate: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/balance-sheet${qs({ asOfDate, currencyCode })}`, null);
    return response.data;
  };

  generalLedger = async (accountId: string, fromDate: string, toDate: string, postingOnly?: boolean) => {
    const response = await api.get(
      `/finance/reports/general-ledger${qs({ accountId, fromDate, toDate, postingOnly })}`,
      null
    );
    return response.data;
  };

  vouchers = async (fromDate?: string, toDate?: string, journalType?: string, status?: string) => {
    const response = await api.get(
      `/finance/reports/vouchers${qs({ fromDate, toDate, journalType, status })}`,
      null
    );
    return response.data;
  };

  chartOfAccounts = async (includeDetailAccounts = true) => {
    const response = await api.get(
      `/finance/reports/chart-of-accounts${qs({ includeDetailAccounts })}`,
      null
    );
    return response.data;
  };

  dailyFinancial = async (date: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/daily-financial${qs({ date, currencyCode })}`, null);
    return response.data;
  };

  cashBankVouchers = async (fromDate: string, toDate: string) => {
    const response = await api.get(
      `/finance/reports/cash-bank-vouchers${qs({ fromDate, toDate })}`,
      null
    );
    return response.data;
  };

  cashBankLedger = async (fromDate: string, toDate: string) => {
    const response = await api.get(
      `/finance/reports/cash-bank-ledger${qs({ fromDate, toDate })}`,
      null
    );
    return response.data;
  };

  voucherTypeMonthly = async (fromDate: string, toDate: string) => {
    const response = await api.get(`/finance/reports/voucher-type-monthly${qs({ fromDate, toDate })}`, null);
    return response.data;
  };

  partnerBalances = async (asOfDate: string) => {
    const response = await api.get(`/finance/reports/partner-balances${qs({ asOfDate })}`, null);
    return response.data;
  };

  agedReceivables = async (asOfDate: string) => {
    const response = await api.get(`/finance/reports/aged-receivables${qs({ asOfDate })}`, null);
    return response.data;
  };

  agedPayables = async (asOfDate: string) => {
    const response = await api.get(`/finance/reports/aged-payables${qs({ asOfDate })}`, null);
    return response.data;
  };

  cashFlow = async (fromDate: string, toDate: string, currencyCode?: string) => {
    const response = await api.get(`/finance/reports/cash-flow${qs({ fromDate, toDate, currencyCode })}`, null);
    return response.data;
  };

  chequeRegister = async (fromDate?: string, toDate?: string, chequeStatus?: string, journalType?: string) => {
    const response = await api.get(
      `/finance/reports/cheque-register${qs({ fromDate, toDate, chequeStatus, journalType })}`,
      null
    );
    return response.data;
  };

  budgetVsActual = async (fromDate: string, toDate: string, budgetId?: string) => {
    const response = await api.get(
      `/finance/reports/budget-vs-actual${qs({ fromDate, toDate, budgetId })}`,
      null
    );
    return response.data;
  };

  /** Inventory-owned WAC utility (deep-linked from Accounts reports). */
  stockAverageRate = async (itemId: string, warehouseId?: string, asOfDate?: string, updateItemCostPrice = false) => {
    const response = await api.get(
      `/inventory/stock/average-rate${qs({ itemId, warehouseId, asOfDate, updateItemCostPrice })}`,
      null
    );
    return response.data;
  };
}

const financeReportService = new FinanceReportService();
export default financeReportService;
