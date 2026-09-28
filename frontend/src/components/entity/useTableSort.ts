import { useCallback, useState } from "react";
import { sortTableData, type SortDirection } from "./sortTableData";

export function useTableSort<T>(defaultKey: string, defaultDir: SortDirection = "asc") {
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

  const sortRows = useCallback(
    (rows: T[], getValue?: (row: T, key: string) => unknown) =>
      sortTableData(rows, sortKey, sortDir, getValue),
    [sortKey, sortDir]
  );

  return { sortKey, sortDir, toggleSort, sortRows, setSortKey, setSortDir };
}
