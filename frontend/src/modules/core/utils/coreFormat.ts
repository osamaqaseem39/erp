import type { AuditLogEntry } from "../services/coreApi";

export const REDACTED = "[redacted]";

export interface FieldChange {
  field: string;
  before: unknown;
  after: unknown;
}

const parse = (json?: string | null): Record<string, unknown> => {
  if (!json) return {};
  try {
    const value: unknown = JSON.parse(json);
    return value && typeof value === "object" && !Array.isArray(value) ? (value as Record<string, unknown>) : {};
  } catch {
    return {};
  }
};

/** Field-by-field before/after from an audit entry's JSON snapshots. */
export function diffAuditEntry(entry: Pick<AuditLogEntry, "oldValues" | "newValues">): FieldChange[] {
  const before = parse(entry.oldValues);
  const after = parse(entry.newValues);
  const fields = Array.from(new Set([...Object.keys(before), ...Object.keys(after)])).filter((f) => f !== "truncated" && f !== "fields");
  return fields.map((field) => ({ field, before: before[field], after: after[field] }));
}


/** Mirrors DocumentNumberService.Format on the server. */
export function formatDocumentNumber(prefix: string, resetPolicy: string, year: number, next: number, padding: number) {
  const digits = String(Math.max(1, Math.floor(next))).padStart(Math.max(1, padding), "0");
  return resetPolicy === "Yearly" ? `${prefix}-${year}-${digits}` : `${prefix}-${digits}`;
}

