import { afterEach, describe, expect, it } from "vitest";
import { formatDate, formatMoney, formatNumber, formatPercent, orDash, setFormatDefaults } from "./format";

describe("format", () => {
  afterEach(() => setFormatDefaults({ locale: undefined, currency: "USD", timeZone: undefined }));

  it("formats money with the value's own currency or the tenant default", () => {
    setFormatDefaults({ locale: "en-US", currency: "PKR" });
    expect(formatMoney(1234.5, "USD")).toBe("$1,234.50");
    expect(formatMoney(10)).toMatch(/PKR|Rs/);
  });

  it("survives an invalid currency code from data", () => {
    setFormatDefaults({ locale: "en-US" });
    expect(formatMoney(5, "XX")).toBe("XX 5.00");
  });

  it("shows a dash for missing values instead of 0 or NaN", () => {
    expect(formatMoney(null)).toBe("—");
    expect(formatNumber("")).toBe("—");
    expect(formatNumber("abc")).toBe("—");
    expect(formatDate(undefined)).toBe("—");
    expect(formatDate("not a date")).toBe("—");
    expect(orDash("")).toBe("—");
  });

  it("formats percentages from fractions or whole numbers", () => {
    setFormatDefaults({ locale: "en-US" });
    expect(formatPercent(0.125)).toBe("12.5%");
    expect(formatPercent(40, { isFraction: false })).toBe("40%");
  });

  it("treats date-only strings as calendar dates, not UTC midnight", () => {
    setFormatDefaults({ locale: "en-US" });
    expect(formatDate("2026-01-10")).toBe("Jan 10, 2026");
  });
});
