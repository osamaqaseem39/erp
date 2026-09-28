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
  referenceNo?: string;
  journalDate: string; // ISO date string
  description?: string;
  currencyCode?: string;
  exchangeRate?: number;
  totalDebit?: number;
  totalCredit?: number;
  status: string;
  createdAt?: string;
  createdBy?: string;
  postedAt?: string;
  postedBy?: string;
  isLocked?: boolean;
  journalEntries?: JournalEntryDto[];
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
  description?: string;
  currencyCode?: string;
  exchangeRate?: number;
  status?: string;
  isLocked?: boolean;
  journalEntries?: CreateJournalEntryDto[];
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
  { value: "General", label: "General" },
  { value: "Adjustment", label: "Adjustment" },
  { value: "Opening", label: "Opening" },
  { value: "Closing", label: "Closing" },
  { value: "Reversal", label: "Reversal" },
];

// Journal Status Options
export const JOURNAL_STATUS_OPTIONS = [
  { value: "Draft", label: "Draft" },
  { value: "Posted", label: "Posted" },
  { value: "Locked", label: "Locked" },
  { value: "Cancelled", label: "Cancelled" },
];

