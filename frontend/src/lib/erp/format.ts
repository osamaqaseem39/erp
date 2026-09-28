/**
 * Display formatting for numbers, money and dates. Every page formats through these helpers so a
 * tenant's locale/currency setting (loaded in Checkpoint 1) changes the whole app in one place.
 */

export interface FormatDefaults {
  /** BCP 47 locale; undefined = browser locale. */
  locale?: string;
  /** ISO 4217 code used when a value has no currency of its own. */
  currency: string;
  timeZone?: string;
}

const defaults: FormatDefaults = { locale: undefined, currency: "USD", timeZone: undefined };

export function setFormatDefaults(next: Partial<FormatDefaults>) {
  Object.assign(defaults, next);
}

export function getFormatDefaults(): Readonly<FormatDefaults> {
  return defaults;
}

const EMPTY = "—";

const isBlank = (value: unknown) => value === null || value === undefined || value === "";

const toNumber = (value: number | string | null | undefined): number | null => {
  if (isBlank(value)) return null;
  const n = typeof value === "number" ? value : Number(value);
  return Number.isFinite(n) ? n : null;
};

export function formatNumber(value: number | string | null | undefined, maximumFractionDigits = 2): string {
  const n = toNumber(value);
  return n === null ? EMPTY : new Intl.NumberFormat(defaults.locale, { maximumFractionDigits }).format(n);
}

export function formatMoney(value: number | string | null | undefined, currency?: string | null): string {
  const n = toNumber(value);
  if (n === null) return EMPTY;
  const code = (currency || defaults.currency).toUpperCase();
  try {
    return new Intl.NumberFormat(defaults.locale, { style: "currency", currency: code }).format(n);
  } catch {
    // Unknown/invalid currency code from data: still show the amount and the code.
    return `${code} ${new Intl.NumberFormat(defaults.locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)}`;
  }
}

/** 0.125 -> "12.5%". Pass `isFraction: false` when the value is already a percentage (12.5). */
export function formatPercent(value: number | string | null | undefined, { isFraction = true, digits = 1 } = {}): string {
  const n = toNumber(value);
  if (n === null) return EMPTY;
  return new Intl.NumberFormat(defaults.locale, { style: "percent", maximumFractionDigits: digits }).format(isFraction ? n : n / 100);
}

const toDate = (value: Date | string | number | null | undefined): Date | null => {
  if (isBlank(value)) return null;
  // Date-only strings ("2026-09-28") are calendar dates, not UTC midnight: parse them as local.
  if (typeof value === "string" && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [y, m, d] = value.split("-").map(Number);
    return new Date(y, m - 1, d);
  }
  const date = value instanceof Date ? value : new Date(value as string | number);
  return Number.isNaN(date.getTime()) ? null : date;
};

export function formatDate(value: Date | string | number | null | undefined): string {
  const date = toDate(value);
  return date
    ? new Intl.DateTimeFormat(defaults.locale, { year: "numeric", month: "short", day: "numeric", timeZone: defaults.timeZone }).format(date)
    : EMPTY;
}

export function formatDateTime(value: Date | string | number | null | undefined): string {
  const date = toDate(value);
  return date
    ? new Intl.DateTimeFormat(defaults.locale, {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "numeric",
        minute: "2-digit",
        timeZone: defaults.timeZone,
      }).format(date)
    : EMPTY;
}

/** Shows `EMPTY` for null/blank text so tables never render empty cells. */
export const orDash = (value: string | null | undefined) => (isBlank(value) ? EMPTY : (value as string));
