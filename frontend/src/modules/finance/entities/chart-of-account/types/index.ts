// ChartOfAccount Entity Types
// Based on ERP.Shared.DTOs.Finance.ChartOfAccountDto

export interface ChartOfAccountDto {
  id?: string;
  tenantId?: string;
  accountId?: string;
  accountCode?: string;
  accountName?: string;
  accountType?: string;
  parentAccountId?: string;
  parentAccountName?: string;
  currencyCode?: string;
  isActive?: boolean;
  isPostingAllowed?: boolean;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateChartOfAccountDto {
  accountCode: string;
  accountName: string;
  accountType: string;
  parentAccountId?: string;
  currencyCode?: string;
  isActive?: boolean;
  isPostingAllowed?: boolean;
}

export interface UpdateChartOfAccountDto {
  id: string;
  accountId?: string;
  accountCode?: string;
  accountName?: string;
  accountType?: string;
  parentAccountId?: string;
  currencyCode?: string;
  isActive?: boolean;
  isPostingAllowed?: boolean;
}

export interface ChartOfAccountQuery {
  accountType?: string;
  isActive?: boolean;
}

// Account Type Options
export const ACCOUNT_TYPE_OPTIONS = [
  { value: "Asset", label: "Asset" },
  { value: "Liability", label: "Liability" },
  { value: "Equity", label: "Equity" },
  { value: "Revenue", label: "Revenue" },
  { value: "Expense", label: "Expense" },
  { value: "CostOfGoodsSold", label: "Cost of Goods Sold" },
  { value: "OtherIncome", label: "Other Income" },
  { value: "OtherExpense", label: "Other Expense" },
];

