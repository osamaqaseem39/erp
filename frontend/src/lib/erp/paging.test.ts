import { describe, expect, it } from "vitest";
import { pageRange, queryFromSearchParams, searchParamsFromQuery, toPagedResult, toRequestParams } from "./paging";

describe("paging", () => {
  it("builds backend params with sortDescending and non-empty filters only", () => {
    expect(
      toRequestParams({
        pageNumber: 2,
        pageSize: 25,
        searchTerm: "  acme ",
        sortBy: "customerName",
        sortDirection: "desc",
        filters: { status: "Active", industry: "" },
      })
    ).toEqual({ pageNumber: 2, pageSize: 25, searchTerm: "acme", sortBy: "customerName", sortDescending: true, status: "Active" });
  });

  it("normalizes a backend page and derives navigation flags", () => {
    const page = toPagedResult({ items: [1, 2], totalCount: 52, pageNumber: 2, pageSize: 25 });
    expect(page).toMatchObject({ totalPages: 3, hasPreviousPage: true, hasNextPage: true });
  });

  it("computes the visible range", () => {
    expect(pageRange({ pageNumber: 3, pageSize: 25, totalCount: 52 })).toEqual({ from: 51, to: 52 });
    expect(pageRange({ pageNumber: 1, pageSize: 25, totalCount: 0 })).toEqual({ from: 0, to: 0 });
  });

  it("round-trips a query through the URL", () => {
    const query = {
      pageNumber: 3,
      pageSize: 50,
      searchTerm: "acme",
      sortBy: "createdAt",
      sortDirection: "desc" as const,
      filters: { status: "Active" },
    };
    const params = searchParamsFromQuery(query);
    expect(params.toString()).toBe("page=3&size=50&q=acme&sort=createdAt&dir=desc&status=Active");
    expect(queryFromSearchParams<{ status: string }>(params, { filterKeys: ["status"] })).toEqual(query);
  });

  it("ignores garbage in the URL", () => {
    const query = queryFromSearchParams(new URLSearchParams("page=-4&size=abc&dir=sideways"), { sortBy: "name" });
    expect(query).toMatchObject({ pageNumber: 1, pageSize: 25, sortBy: "name", sortDirection: "asc" });
  });

  it("keeps default values out of the URL", () => {
    expect(searchParamsFromQuery({ pageNumber: 1, pageSize: 25 }).toString()).toBe("");
  });
});
