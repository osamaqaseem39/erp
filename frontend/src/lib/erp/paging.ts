/** Mirrors ERP.Shared.DTOs.PagedRequest / PagedResult<T>. */

export type SortDirection = "asc" | "desc";

export interface PagedQuery<TFilters extends object = Record<string, never>> {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortBy?: string;
  sortDirection?: SortDirection;
  filters?: Partial<TFilters>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100] as const;
export const DEFAULT_PAGE_SIZE = 25;

/** Query-string params for a "/paged" endpoint (backend expects SortDescending, flat filters). */
export function toRequestParams(query: PagedQuery<object>): Record<string, string | number | boolean> {
  const params: Record<string, string | number | boolean> = {
    pageNumber: query.pageNumber,
    pageSize: query.pageSize,
  };
  if (query.searchTerm?.trim()) params.searchTerm = query.searchTerm.trim();
  if (query.sortBy) {
    params.sortBy = query.sortBy;
    params.sortDescending = query.sortDirection === "desc";
  }
  for (const [key, value] of Object.entries(query.filters ?? {})) {
    if (value !== undefined && value !== null && value !== "") params[key] = value as string | number | boolean;
  }
  return params;
}

/** Normalizes a backend PagedResult (camelCase JSON) and tolerates a missing optional field. */
export function toPagedResult<T>(data: Partial<PagedResult<T>> | null | undefined): PagedResult<T> {
  const items = data?.items ?? [];
  const pageSize = data?.pageSize && data.pageSize > 0 ? data.pageSize : DEFAULT_PAGE_SIZE;
  const pageNumber = data?.pageNumber && data.pageNumber > 0 ? data.pageNumber : 1;
  const totalCount = data?.totalCount ?? items.length;
  const totalPages = data?.totalPages ?? Math.ceil(totalCount / pageSize);
  return {
    items,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
    hasPreviousPage: pageNumber > 1,
    hasNextPage: pageNumber < totalPages,
  };
}

/** "Showing 26–50 of 312" range for a page. */
export function pageRange(result: Pick<PagedResult<unknown>, "pageNumber" | "pageSize" | "totalCount">) {
  if (result.totalCount === 0) return { from: 0, to: 0 };
  const from = (result.pageNumber - 1) * result.pageSize + 1;
  return { from, to: Math.min(result.pageNumber * result.pageSize, result.totalCount) };
}

// ---- URL <-> query, so filters survive refresh, back navigation and shared links ----

const RESERVED = new Set(["page", "size", "q", "sort", "dir"]);

export function queryFromSearchParams<TFilters extends object>(
  params: URLSearchParams,
  defaults: { pageSize?: number; sortBy?: string; sortDirection?: SortDirection; filterKeys?: readonly (keyof TFilters & string)[] } = {}
): PagedQuery<TFilters> {
  const positiveInt = (raw: string | null, fallback: number) => {
    const n = Number(raw);
    return Number.isInteger(n) && n > 0 ? n : fallback;
  };
  const filters: Record<string, string> = {};
  for (const key of defaults.filterKeys ?? []) {
    const value = params.get(key);
    if (value && !RESERVED.has(key)) filters[key] = value;
  }
  const dir = params.get("dir");
  return {
    pageNumber: positiveInt(params.get("page"), 1),
    pageSize: positiveInt(params.get("size"), defaults.pageSize ?? DEFAULT_PAGE_SIZE),
    searchTerm: params.get("q") ?? undefined,
    sortBy: params.get("sort") ?? defaults.sortBy,
    sortDirection: dir === "asc" || dir === "desc" ? dir : defaults.sortDirection ?? "asc",
    filters: filters as Partial<TFilters>,
  };
}

export function searchParamsFromQuery(query: PagedQuery<object>, defaults: { pageSize?: number } = {}): URLSearchParams {
  const params = new URLSearchParams();
  if (query.pageNumber > 1) params.set("page", String(query.pageNumber));
  if (query.pageSize !== (defaults.pageSize ?? DEFAULT_PAGE_SIZE)) params.set("size", String(query.pageSize));
  if (query.searchTerm?.trim()) params.set("q", query.searchTerm.trim());
  if (query.sortBy) {
    params.set("sort", query.sortBy);
    params.set("dir", query.sortDirection ?? "asc");
  }
  for (const [key, value] of Object.entries(query.filters ?? {})) {
    if (value !== undefined && value !== null && value !== "") params.set(key, String(value));
  }
  return params;
}
