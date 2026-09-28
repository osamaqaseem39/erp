import { getFormatDefaults } from "./format";

export type KpiFormat = "number" | "money" | "percent";

/** 1,284 / 12.9K / $4.2M: compact display for tiles (tables use full values). */
export function formatKpiValue(value: number, format: KpiFormat = "number", currency?: string): string {
  const { locale, currency: defaultCurrency } = getFormatDefaults();
  if (format === "percent") {
    return new Intl.NumberFormat(locale, { style: "percent", maximumFractionDigits: 1 }).format(value);
  }
  const compact = Math.abs(value) >= 10_000;
  return new Intl.NumberFormat(locale, {
    notation: compact ? "compact" : "standard",
    maximumFractionDigits: compact ? 1 : format === "money" ? 2 : 0,
    ...(format === "money" ? { style: "currency", currency: (currency || defaultCurrency).toUpperCase() } : {}),
  }).format(value);
}

export interface KpiDelta {
  direction: "up" | "down" | "flat";
  /** Relative change; null when the previous value was 0 (no meaningful percentage). */
  percent: number | null;
  sentiment: "good" | "bad" | "neutral";
}

export function computeDelta(current: number, previous: number, higherIsBetter = true): KpiDelta {
  const diff = current - previous;
  const direction = diff > 0 ? "up" : diff < 0 ? "down" : "flat";
  const percent = previous === 0 ? null : diff / Math.abs(previous);
  const sentiment = direction === "flat" ? "neutral" : (direction === "up") === higherIsBetter ? "good" : "bad";
  return { direction, percent, sentiment };
}
