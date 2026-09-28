/**
 * Line-item math for quotes, orders, invoices and bills. Display preview only: the server's totals
 * are authoritative. Amounts are computed in integer cents so repeated sums don't drift.
 *
 * Per line: net = qty × unit price; discount = net × discount%; tax = (net − discount) × tax%.
 */

export interface LineInput {
  quantity: number | string | null | undefined;
  unitPrice: number | string | null | undefined;
  discountPercent?: number | string | null;
  taxPercent?: number | string | null;
}

export interface LineAmounts {
  net: number;
  discount: number;
  tax: number;
  total: number;
}

export interface Totals {
  lines: LineAmounts[];
  subtotal: number;
  discountTotal: number;
  taxTotal: number;
  grandTotal: number;
}

const num = (value: number | string | null | undefined) => {
  const n = typeof value === "number" ? value : Number(value);
  return Number.isFinite(n) ? n : 0;
};

/** Round to whole cents, half away from zero (0.005 -> 0.01, -0.005 -> -0.01). */
const toCents = (amount: number) => Math.sign(amount) * Math.round(Math.abs(amount) * 100 + 1e-7);
const fromCents = (cents: number) => cents / 100;

export function lineTotals(inputs: LineInput[]): Totals {
  let subtotal = 0;
  let discountTotal = 0;
  let taxTotal = 0;

  const lines = inputs.map((line) => {
    const qty = num(line.quantity);
    const price = num(line.unitPrice);
    const discountRate = Math.min(Math.max(num(line.discountPercent), 0), 100) / 100;
    const taxRate = Math.max(num(line.taxPercent), 0) / 100;

    const netCents = toCents(qty * price);
    const discountCents = toCents(fromCents(netCents) * discountRate);
    const taxCents = toCents(fromCents(netCents - discountCents) * taxRate);
    const totalCents = netCents - discountCents + taxCents;

    subtotal += netCents;
    discountTotal += discountCents;
    taxTotal += taxCents;
    return { net: fromCents(netCents), discount: fromCents(discountCents), tax: fromCents(taxCents), total: fromCents(totalCents) };
  });

  return {
    lines,
    subtotal: fromCents(subtotal),
    discountTotal: fromCents(discountTotal),
    taxTotal: fromCents(taxTotal),
    grandTotal: fromCents(subtotal - discountTotal + taxTotal),
  };
}
