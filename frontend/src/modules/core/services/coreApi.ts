import { APICore } from "../../../helpers/api/apiCore";
import { toPagedResult, toRequestParams, type PagedQuery, type PagedResult } from "../../../lib/erp/paging";

const api = new APICore();

// ---------- types (mirror ERP.Shared.DTOs.Core / Admin) ----------

export interface WidgetResult<T> {
  data?: T | null;
  error?: string | null;
}

export interface CoreDashboard {
  users: WidgetResult<{ totalUsers: number; activeLast7Days: number; activePrevious7Days: number; lockedAccounts: number }>;
  security: WidgetResult<{ failedSignInsLast24h: number; failedSignInsPrevious24h: number; lockouts7Days: number }>;
  errors: WidgetResult<{ errorsLast24h: number; errorsPrevious24h: number; latest: { id: string; timestamp: string; message: string; level?: string }[] }>;
  configChanges: WidgetResult<{ id: string; timestamp: string; entityName: string; action: string; userName?: string | null }[]>;
  setup: WidgetResult<{ key: string; label: string; done: boolean; link: string }[]>;
  tenants?: WidgetResult<{ status: string; count: number }[]> | null;
}

export interface CompanyProfile {
  id: string;
  companyName: string;
  legalName?: string | null;
  registrationNumber?: string | null;
  taxId?: string | null;
  addressLine1?: string | null;
  addressLine2?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  phone?: string | null;
  mobilePhone?: string | null;
  contactPerson?: string | null;
  email?: string | null;
  website?: string | null;
  logoUrl?: string | null;
  currency: string;
  timeZone: string;
}

export type UpdateCompanyProfile = Omit<CompanyProfile, "id">;

export interface TenantContext {
  companyName: string;
  currency: string;
  timeZone: string;
  locale: string;
  dateFormat: string;
  firstDayOfWeek: number;
}

export type SettingType = "text" | "number" | "boolean" | "choice" | "email" | "secret";

export interface SettingValue {
  key: string;
  label: string;
  type: SettingType;
  value?: string | null;
  hasValue: boolean;
  isDefault: boolean;
  description?: string | null;
  required: boolean;
  readOnly: boolean;
  min?: number | null;
  max?: number | null;
  options?: { value: string; label: string }[] | null;
}

export interface SettingsCategory {
  category: string;
  settings: SettingValue[];
}

export interface DocumentSequence {
  key: string;
  label: string;
  prefix: string;
  padding: number;
  resetPolicy: "Never" | "Yearly";
  nextNumber: number;
  preview: string;
}

export interface AuditLogEntry {
  id: string;
  tenantId: string;
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string | null;
  newValues?: string | null;
  userId?: string | null;
  userName?: string | null;
  ipAddress?: string | null;
  timestamp: string;
}

export interface AuditLogFilters {
  entityName: string;
  action: string;
  from: string;
  to: string;
}

export interface SystemLogEntry {
  id: string;
  logLevel: string;
  message: string;
  exception?: string | null;
  source?: string | null;
  userId?: string | null;
  ipAddress?: string | null;
  additionalData?: string | null;
  createdAt: string;
}

export interface SystemLogFilters {
  logLevel: string;
  source: string;
  from: string;
  to: string;
}

// ---------- calls ----------

export const coreApi = {
  dashboard: async (signal?: AbortSignal): Promise<CoreDashboard> => (await api.get("/core/dashboard/summary", null, { signal })).data,

  company: async (signal?: AbortSignal): Promise<CompanyProfile> => (await api.get("/core/company", null, { signal })).data,
  updateCompany: async (profile: UpdateCompanyProfile): Promise<CompanyProfile> => (await api.update("/core/company", profile)).data,
  context: async (signal?: AbortSignal): Promise<TenantContext> => (await api.get("/core/context", null, { signal })).data,

  settings: async (category: string, signal?: AbortSignal): Promise<SettingsCategory> =>
    (await api.get(`/core/settings/${category}`, null, { signal })).data,
  /** Omit a secret's key (or send null) to keep it; send "" to clear it. */
  updateSettings: async (category: string, values: Record<string, string | null>): Promise<SettingsCategory> =>
    (await api.update(`/core/settings/${category}`, { values })).data,

  sequences: async (signal?: AbortSignal): Promise<DocumentSequence[]> => (await api.get("/core/sequences", null, { signal })).data,
  updateSequence: async (key: string, value: Pick<DocumentSequence, "prefix" | "padding" | "resetPolicy" | "nextNumber">): Promise<DocumentSequence> =>
    (await api.update(`/core/sequences/${key}`, value)).data,

  auditLogs: async (query: PagedQuery<AuditLogFilters>, signal?: AbortSignal): Promise<PagedResult<AuditLogEntry>> =>
    toPagedResult((await api.get("/admin/audit-logs/paged", toRequestParams(query), { signal })).data),
  auditLogsForEntity: async (entityName: string, entityId: string, signal?: AbortSignal): Promise<AuditLogEntry[]> =>
    (await api.get(`/admin/audit-logs/entity/${encodeURIComponent(entityName)}/${encodeURIComponent(entityId)}`, null, { signal })).data,

  systemLogs: async (query: PagedQuery<SystemLogFilters>, signal?: AbortSignal): Promise<PagedResult<SystemLogEntry>> =>
    toPagedResult((await api.get("/admin/system-logs/paged", toRequestParams(query), { signal })).data),
};
