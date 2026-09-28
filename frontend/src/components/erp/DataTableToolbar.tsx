import type { ReactNode } from "react";

export interface DataTableToolbarProps {
  /** Usually a <SearchInput>. */
  search?: ReactNode;
  /** Quick filters shown inline (a status select), plus the "Filters" drawer button. */
  filters?: ReactNode;
  /** Column selector, export, etc. */
  tools?: ReactNode;
  onRefresh?: () => void;
  isRefreshing?: boolean;
}

export function DataTableToolbar({ search, filters, tools, onRefresh, isRefreshing }: DataTableToolbarProps) {
  return (
    <div className="flex flex-wrap items-center gap-2 p-4">
      {search && <div className="min-w-[14rem] flex-1">{search}</div>}
      {filters}
      <div className="ms-auto flex items-center gap-2">
        {tools}
        {onRefresh && (
          <button
            type="button"
            className="btn border border-slate-300 dark:border-slate-600"
            onClick={onRefresh}
            disabled={isRefreshing}
            aria-label="Refresh"
          >
            <i className={`mgc_refresh_1_line ${isRefreshing ? "animate-spin" : ""}`} aria-hidden="true" />
          </button>
        )}
      </div>
    </div>
  );
}

/** Replaces the toolbar while rows are selected. */
export function BulkActionBar({ count, onClear, children }: { count: number; onClear: () => void; children: ReactNode }) {
  if (count === 0) return null;
  return (
    <div className="flex flex-wrap items-center gap-3 border-b border-primary/20 bg-primary/5 px-4 py-2 text-sm" role="region" aria-label="Bulk actions">
      <span className="font-medium">{count} selected</span>
      <div className="flex flex-wrap items-center gap-2">{children}</div>
      <button type="button" className="ms-auto text-slate-600 underline" onClick={onClear}>
        Clear selection
      </button>
    </div>
  );
}
