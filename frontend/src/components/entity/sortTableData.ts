export type SortDirection = "asc" | "desc";

export function compareValues(a: unknown, b: unknown): number {
  if (a == null && b == null) return 0;
  if (a == null) return -1;
  if (b == null) return 1;

  if (typeof a === "number" && typeof b === "number") {
    return a - b;
  }

  if (typeof a === "boolean" && typeof b === "boolean") {
    return Number(a) - Number(b);
  }

  if (typeof a === "string" && typeof b === "string") {
    const da = Date.parse(a);
    const db = Date.parse(b);
    if (!Number.isNaN(da) && !Number.isNaN(db) && /^\d{4}-\d{2}-\d{2}/.test(a) && /^\d{4}-\d{2}-\d{2}/.test(b)) {
      return da - db;
    }
    return a.localeCompare(b, undefined, { numeric: true, sensitivity: "base" });
  }

  return String(a).localeCompare(String(b), undefined, { numeric: true, sensitivity: "base" });
}

export function sortTableData<T>(
  rows: T[],
  sortKey: string,
  sortDir: SortDirection,
  getValue?: (row: T, key: string) => unknown
): T[] {
  const read =
    getValue ??
    ((row: T, key: string) => (row as Record<string, unknown>)[key]);

  return [...rows].sort((a, b) => {
    const cmp = compareValues(read(a, sortKey), read(b, sortKey));
    return sortDir === "asc" ? cmp : -cmp;
  });
}
