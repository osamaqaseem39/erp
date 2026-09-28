import { describe, expect, it } from "vitest";
import { lineTotals } from "./lineTotals";

describe("lineTotals", () => {
  it("applies discount before tax per line", () => {
    const t = lineTotals([{ quantity: 2, unitPrice: 50, discountPercent: 10, taxPercent: 17 }]);
    // net 100, discount 10, taxable 90, tax 15.30
    expect(t.lines[0]).toEqual({ net: 100, discount: 10, tax: 15.3, total: 105.3 });
    expect(t).toMatchObject({ subtotal: 100, discountTotal: 10, taxTotal: 15.3, grandTotal: 105.3 });
  });

  it("does not drift with amounts that are inexact in floating point", () => {
    const t = lineTotals(Array.from({ length: 10 }, () => ({ quantity: 1, unitPrice: 0.1 })));
    expect(t.grandTotal).toBe(1);
  });

  it("rounds each line to cents, half away from zero", () => {
    const t = lineTotals([{ quantity: 1, unitPrice: 0.125 }, { quantity: 3, unitPrice: 1.005 }]);
    expect(t.lines.map((l) => l.net)).toEqual([0.13, 3.02]);
  });

  it("treats blank or invalid input as zero and clamps discount to 0–100%", () => {
    const t = lineTotals([
      { quantity: "", unitPrice: "abc" },
      { quantity: 1, unitPrice: 10, discountPercent: 150 },
      { quantity: 1, unitPrice: 10, discountPercent: -5, taxPercent: -3 },
    ]);
    expect(t.lines.map((l) => l.total)).toEqual([0, 0, 10]);
  });

  it("handles string numbers from form inputs", () => {
    expect(lineTotals([{ quantity: "3", unitPrice: "19.99", taxPercent: "5" }]).grandTotal).toBe(62.97);
  });

  it("returns zeros for no lines", () => {
    expect(lineTotals([])).toEqual({ lines: [], subtotal: 0, discountTotal: 0, taxTotal: 0, grandTotal: 0 });
  });
});
