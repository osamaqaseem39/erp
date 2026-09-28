import { describe, expect, it } from "vitest";
import { safeRedirectPath } from "./safeRedirect";

describe("safeRedirectPath", () => {
  it("reads next wherever it is in the query", () => {
    expect(safeRedirectPath("?reason=session_expired&next=%2Fcrm%2Fcustomers%3Fpage%3D2")).toBe("/crm/customers?page=2");
  });

  it.each(["?next=https://evil.test", "?next=//evil.test", "?next=/\\evil.test", "?next=%2F%5Cevil.test","?next=javascript:alert(1)", "", "?next="])(
    "falls back for %s",
    (search) => {
      expect(safeRedirectPath(search)).toBe("/modules");
    }
  );
});
