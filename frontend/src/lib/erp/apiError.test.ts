import { describe, expect, it } from "vitest";
import { parseApiError, toFieldPath, unassignedErrors, DEFAULT_ERROR_MESSAGE } from "./apiError";

// Mirrors what helpers/api/apiCore.ts rejects with: an Error carrying the axios response.
const httpError = (status: number, data?: unknown) =>
  Object.assign(new Error("wrapped by interceptor"), { status, response: { status, data } });

describe("parseApiError", () => {
  it("reads the current ApiError contract", () => {
    const info = parseApiError(
      httpError(400, {
        status: 400,
        message: "Validation failed",
        errorCode: "VALIDATION_ERROR",
        errors: { customerCode: ["Customer code is required"], "lines[0].unitPrice": ["Must be 0 or greater"] },
        traceId: "t-1",
      })
    );
    expect(info).toMatchObject({ status: 400, message: "Validation failed", code: "VALIDATION_ERROR", traceId: "t-1" });
    expect(info.fieldErrors).toEqual({
      customerCode: ["Customer code is required"],
      "lines[0].unitPrice": ["Must be 0 or greater"],
    });
  });

  it("reads legacy FluentValidation failure arrays", () => {
    const info = parseApiError(
      httpError(400, { message: "Validation failed", errors: [{ propertyName: "Email", errorMessage: "Enter a valid email" }] })
    );
    expect(info.fieldErrors).toEqual({ email: ["Enter a valid email"] });
  });

  it("reads ASP.NET ProblemDetails with PascalCase keys", () => {
    const info = parseApiError(
      httpError(400, { title: "One or more validation errors occurred.", errors: { "$.CreditLimit": ["Invalid number"] } })
    );
    expect(info.message).toBe("One or more validation errors occurred.");
    expect(info.fieldErrors).toEqual({ creditLimit: ["Invalid number"] });
  });

  it("reads legacy { Error } and plain string bodies", () => {
    expect(parseApiError(httpError(400, { Error: "Code already exists" })).message).toBe("Code already exists");
    expect(parseApiError(httpError(400, "ID mismatch")).message).toBe("ID mismatch");
  });

  it.each([
    [401, "Your session has expired. Please sign in again."],
    [403, "You don't have permission to do this."],
    [404, "This record could not be found. It may have been deleted."],
    [501, "This feature is not available yet."],
  ])("falls back to a readable message for %i", (status, message) => {
    expect(parseApiError(httpError(status)).message).toBe(message);
  });

  it("never shows server text for 5xx", () => {
    const info = parseApiError(httpError(500, { message: "SqlException: Login failed for user sa", traceId: "t-9" }));
    expect(info.message).toBe(DEFAULT_ERROR_MESSAGE);
    expect(info.traceId).toBe("t-9");
  });

  it("recognizes network failures and cancellations", () => {
    expect(parseApiError(Object.assign(new Error("Network Error"), { code: "ERR_NETWORK" })).isNetworkError).toBe(true);
    expect(parseApiError(Object.assign(new Error("canceled"), { name: "CanceledError" })).isCanceled).toBe(true);
  });

  it("does not show client-side exception text", () => {
    const info = parseApiError(new TypeError("Cannot read properties of undefined (reading 'id')"));
    expect(info.message).toBe(DEFAULT_ERROR_MESSAGE);
    expect(info.isNetworkError).toBe(false);
  });

  it("handles non-error values", () => {
    expect(parseApiError(undefined).message).toBe(DEFAULT_ERROR_MESSAGE);
    expect(parseApiError("boom").message).toBe(DEFAULT_ERROR_MESSAGE);
  });
});

describe("helpers", () => {
  it("toFieldPath camel-cases each segment", () => {
    expect(toFieldPath("Lines[0].UnitPrice")).toBe("lines[0].unitPrice");
    expect(toFieldPath("$.email")).toBe("email");
    expect(toFieldPath("")).toBe("");
  });

  it("unassignedErrors returns messages for fields the form does not show", () => {
    const info = parseApiError(httpError(400, { errors: { email: ["bad"], "": ["General problem"], secret: ["x"] } }));
    expect(unassignedErrors(info, ["email"])).toEqual(["General problem", "x"]);
  });
});
