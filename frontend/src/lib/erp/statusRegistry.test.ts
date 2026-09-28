import { describe, expect, it } from "vitest";
import { activeStatus, humanizeStatus, resolveStatus } from "./statusRegistry";

describe("resolveStatus", () => {
  it.each([
    ["Draft", "neutral"],
    ["PendingApproval", "warning"],
    ["pending_approval", "warning"],
    ["In Progress", "info"],
    ["Paid", "success"],
    ["Overdue", "danger"],
    ["CANCELLED", "danger"],
  ])("%s is %s", (status, tone) => {
    expect(resolveStatus(status).tone).toBe(tone);
  });

  it("gives every tone an icon so color is not the only signal", () => {
    expect(resolveStatus("Approved").icon).toMatch(/^mgc_/);
  });

  it("renders unknown statuses as neutral with a humanized label", () => {
    expect(resolveStatus("AwaitingCourier")).toMatchObject({ tone: "neutral", label: "Awaiting courier" });
  });

  it("allows module relabeling but not recoloring", () => {
    expect(resolveStatus("Posted", { posted: "Booked" })).toMatchObject({ label: "Booked", tone: "success" });
  });

  it("handles empty values", () => {
    expect(resolveStatus(null).label).toBe("—");
  });

  it("maps booleans onto the same vocabulary", () => {
    expect(resolveStatus(activeStatus(false)).label).toBe("Inactive");
  });

  it("humanizes separators and camel case", () => {
    expect(humanizeStatus("PartiallyPaid")).toBe("Partially paid");
    expect(humanizeStatus("on-hold")).toBe("On hold");
  });
});
