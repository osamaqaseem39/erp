import { afterEach, describe, expect, it } from "vitest";
import { computeDelta, formatKpiValue } from "./kpi";
import { setFormatDefaults } from "./format";
import { visiblePages } from "./paging";

describe("computeDelta", () => {
  it("marks an increase good when higher is better", () => {
    expect(computeDelta(120, 100)).toEqual({ direction: "up", percent: 0.2, sentiment: "good" });
  });

  it("marks an increase bad when higher is worse (overdue invoices)", () => {
    expect(computeDelta(12, 10, false)).toMatchObject({ direction: "up", sentiment: "bad" });
  });

  it("has no percentage when the previous value was zero", () => {
    expect(computeDelta(5, 0)).toMatchObject({ direction: "up", percent: null });
  });

  it("is neutral when unchanged", () => {
    expect(computeDelta(7, 7)).toMatchObject({ direction: "flat", sentiment: "neutral" });
  });
});

describe("formatKpiValue", () => {
  afterEach(() => setFormatDefaults({ locale: undefined, currency: "USD" }));

  it("compacts large values and keeps small ones exact", () => {
    setFormatDefaults({ locale: "en-US" });
    expect(formatKpiValue(1284)).toBe("1,284");
    expect(formatKpiValue(12_900)).toBe("12.9K");
    expect(formatKpiValue(4_200_000, "money", "USD")).toBe("$4.2M");
    expect(formatKpiValue(0.253, "percent")).toBe("25.3%");
  });
});

describe("visiblePages", () => {
  it("lists every page when there are few", () => {
    expect(visiblePages(2, 5)).toEqual([1, 2, 3, 4, 5]);
  });

  it("elides the middle around the current page", () => {
    expect(visiblePages(10, 20)).toEqual([1, null, 9, 10, 11, null, 20]);
    expect(visiblePages(1, 20)).toEqual([1, 2, 3, 4, null, 20]);
    expect(visiblePages(20, 20)).toEqual([1, null, 17, 18, 19, 20]);
  });
});
