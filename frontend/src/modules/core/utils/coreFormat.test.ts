import { describe, expect, it } from "vitest";
import { diffAuditEntry, formatDocumentNumber } from "./coreFormat";

describe("diffAuditEntry", () => {
  it("pairs before and after per field for updates", () => {
    expect(diffAuditEntry({ oldValues: '{"Status":"Active"}', newValues: '{"Status":"Suspended"}' })).toEqual([
      { field: "Status", before: "Active", after: "Suspended" },
    ]);
  });

  it("handles creates and deletes (one side empty)", () => {
    expect(diffAuditEntry({ oldValues: "", newValues: '{"CustomerCode":"C-1"}' })).toEqual([{ field: "CustomerCode", before: undefined, after: "C-1" }]);
    expect(diffAuditEntry({ oldValues: '{"Name":"X"}', newValues: null })).toEqual([{ field: "Name", before: "X", after: undefined }]);
  });

  it("ignores malformed JSON and the truncation marker", () => {
    expect(diffAuditEntry({ oldValues: "not json", newValues: '{"truncated":true,"fields":["A"]}' })).toEqual([]);
  });
});

describe("formatDocumentNumber (mirrors the server)", () => {
  it("formats yearly and never-reset sequences", () => {
    expect(formatDocumentNumber("QT", "Yearly", 2026, 42, 5)).toBe("QT-2026-00042");
    expect(formatDocumentNumber("CU", "Never", 2026, 7, 3)).toBe("CU-007");
  });

  it("never shows zero or fractional numbers", () => {
    expect(formatDocumentNumber("QT", "Never", 2026, 0, 2)).toBe("QT-01");
    expect(formatDocumentNumber("QT", "Never", 2026, 3.7, 1)).toBe("QT-3");
  });
});
