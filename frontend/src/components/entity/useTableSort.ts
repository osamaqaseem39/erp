import { useCallback, useState } from "react";
import { sortTableData, type SortDirection } from "./sortTableData";

/** @deprecated Legacy layout kept until its module is migrated. Use DataTable with usePagedQuery (server-side sort). */
export function useTableSort(defaultKey: string, defaultDir: SortDirection = "asc") {
  const [sortKey, setSortKey] = useState<string>(defaultKey);
  const [sortDir, setSortDir] = useState<SortDirection>(defaultDir);

  const toggleSort = useCallback(
    (key: string) => {
      if (sortKey === key) {
        setSortDir((d) => (d === "asc" ? "desc" : "asc"));
      } else {
        setSortKey(key);
        setSortDir("asc");
      }
    },
    [sortKey]
  );

  // Generic per call so callers get their row type back without annotating the hook.
  const sortRows = useCallback(
    <T,>(rows: T[], getValue?: (row: T, key: string) => unknown): T[] =>
      sortTableData(rows, sortKey, sortDir, getValue),
    [sortKey, sortDir]
  );

  return { sortKey, sortDir, toggleSort, sortRows, setSortKey, setSortDir };
}
