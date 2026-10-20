// Journal Entity Types
// Based on ERP.Shared.DTOs.Finance.JournalDto

export interface JournalEntryDto {
  entryId: string;
  journalId: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  costCenterId?: string;
  costCenterName?: string;
  projectId?: string;
  debit: number;
  credit: number;
  narration?: string;
  entityReferenceTable?: string;
  entityReferenceId?: string;
}

export interface JournalDto {
  id?: string;
  tenantId?: string;
  journalId?: string;
  journalType: string;
  journalNumber?: string;
  referenceNo?: string;
  journalDate: string; // ISO date string
  referenceDate?: string;
  description?: string;
  currencyCode?: string;
  exchangeRate?: number;
  totalDebit?: number;
  totalCredit?: number;
  status: string;
  chequeNumber?: string;
  paidBy?: string;
  receivedBy?: string;
  cashOrBankAccountId?: string;
  createdAt?: string;
  createdBy?: string;
  postedAt?: string;
  postedBy?: string;
  approvedAt?: string;
  approvedBy?: string;
  isLocked?: boolean;
  journalEntries?: JournalEntryDto[];
}

export interface CreateCashPaymentLineDto {
  accountId: string;
  amount: number;
  narration?: string;
  costCenterId?: string;
}

export interface CreateJournalEntryDto {
  journalId: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  costCenterId?: string;
  projectId?: string;
  debit: number;
  credit: number;
  narration?: string;
  entityReferenceTable?: string;
  entityReferenceId?: string;
}

export interface CreateJournalDto {
  journalType: string;
  referenceNo?: string;
  journalDate: string; // ISO date string
  referenceDate?: string;
  description?: string;
  currencyCode?: string;
  exchangeRate?: number;
  chequeNumber?: string;
  paidBy?: string;
  receivedBy?: string;
  cashOrBankAccountId?: string;
  status?: string;
  isLocked?: boolean;
  journalEntries?: CreateJournalEntryDto[];
  debitLines?: CreateCashPaymentLineDto[];
}

export interface UpdateJournalEntryDto {
  entryId?: string;
  accountId?: string;
  accountCode?: string;
  accountName?: string;
  costCenterId?: string;
  projectId?: string;
  debit?: number;
  credit?: number;
  narration?: string;
  entityReferenceTable?: string;
  entityReferenceId?: string;
}

export interface UpdateJournalDto {
  journalId?: string;
  journalType?: string;
  referenceNo?: string;
  journalDate?: string;
  description?: string;
  currencyCode?: string;
  exchangeRate?: number;
  status?: string;
  isLocked?: boolean;
}

export interface JournalQuery {
  tenantId?: string;
  journalType?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Journal Type Options
export const JOURNAL_TYPE_OPTIONS = [
  { value: "CashPayment", label: "Cash Payment" },
  { value: "CashReceipt", label: "Cash Receipt" },
  { value: "BankPayment", label: "Bank Payment" },
  { value: "BankReceipt", label: "Bank Receipt" },
  { value: "Journal", label: "Journal Voucher" },
  { value: "Adjustment", label: "Adjustment" },
  { value: "OpeningBalance", label: "Opening Balance" },
  { value: "Sales", label: "Sales" },
  { value: "Purchase", label: "Purchase" },
];

// Journal Status Options
export const JOURNAL_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Approved", label: "Approved (Closed)" },
  { value: "Posted", label: "Posted" },
  { value: "Reversed", label: "Reversed" },
  { value: "Cancelled", label: "Cancelled" },
];

