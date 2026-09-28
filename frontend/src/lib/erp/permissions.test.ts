import { describe, expect, it } from "vitest";
import { hasPermission, readPermissionContext } from "./permissions";

describe("permissions", () => {
  it("denies everything when signed out", () => {
    expect(hasPermission(readPermissionContext(null), "crm.customers.view")).toBe(false);
  });

  it("allows SuperAdmin everything", () => {
    const ctx = readPermissionContext({ token: "t", roleNames: ["superadmin"], permissions: [] });
    expect(hasPermission(ctx, "finance.journals.post")).toBe(true);
  });

  it("allows any signed-in user until the backend sends permissions", () => {
    expect(hasPermission(readPermissionContext({ token: "t", roleNames: ["User"] }), "hrm.payroll.view")).toBe(true);
  });

  it("enforces the list once the backend sends permissions", () => {
    const ctx = readPermissionContext({ token: "t", roleNames: ["User"], permissions: ["crm.customers.view", "HRM.*"] });
    expect(hasPermission(ctx, "crm.customers.view")).toBe(true);
    expect(hasPermission(ctx, "crm.customers.delete")).toBe(false);
    expect(hasPermission(ctx, "hrm.payroll.approve")).toBe(true);
    expect(hasPermission(ctx, "finance.invoices.view")).toBe(false);
  });

  it("supports resource wildcards", () => {
    const ctx = readPermissionContext({ token: "t", permissions: ["crm.customers.*"] });
    expect(hasPermission(ctx, "crm.customers.export")).toBe(true);
    expect(hasPermission(ctx, "crm.leads.view")).toBe(false);
  });
});
