import { useCallback, useMemo, useState } from "react";

/** Minimal column shape the hook needs (DataTable columns satisfy it). */
interface ColumnLike {
  key: string;
  defaultHidden?: boolean;
}

const STORAGE_PREFIX = "erp.columns.";

function readStored(key: string): string[] | null {
  try {
    const raw = window.localStorage.getItem(STORAGE_PREFIX + key);
    const parsed: unknown = raw ? JSON.parse(raw) : null;
    return Array.isArray(parsed) && parsed.every((v) => typeof v === "string") ? parsed : null;
  } catch {
    return null;
  }
}

function writeStored(key: string, hidden: string[]) {
  try {
    window.localStorage.setItem(STORAGE_PREFIX + key, JSON.stringify(hidden));
  } catch {
    // Private mode / storage disabled: the choice just won't persist.
  }
}

/**
 * Which columns are hidden, remembered per table in this browser.
 * Storage is a convenience only; the table works identically without it.
 */
export function useColumnVisibility(storageKey: string, columns: readonly ColumnLike[]) {
  const defaults = useMemo(() => columns.filter((c) => c.defaultHidden).map((c) => c.key), [columns]);
  const [hidden, setHidden] = useState<Set<string>>(() => new Set(readStored(storageKey) ?? defaults));

  const toggle = useCallback(
    (key: string) => {
      setHidden((prev) => {
        const next = new Set(prev);
        if (next.has(key)) next.delete(key);
        else next.add(key);
        writeStored(storageKey, [...next]);
        return next;
      });
    },
    [storageKey]
  );

  const reset = useCallback(() => {
    setHidden(new Set(defaults));
    writeStored(storageKey, defaults);
  }, [defaults, storageKey]);

  return { hidden: hidden as ReadonlySet<string>, toggle, reset };
}
