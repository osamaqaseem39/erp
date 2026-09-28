import { useId, useState, type ReactNode } from "react";
import type { ApiErrorInfo } from "../../lib/erp/apiError";
import { EmptyState } from "./EmptyState";
import { ErrorState } from "./ErrorState";


export interface ChartCardProps {
  title: string;
  subtitle?: string;
  /** The chart itself (ApexCharts). Only rendered when there is data. */
  children: ReactNode;
  /** The same numbers as a table: charts are never the only way to read the data. */
  table?: ReactNode;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  error?: ApiErrorInfo | null;
  onRetry?: () => void;
  unavailable?: boolean;
  actions?: ReactNode;
}

export function ChartCard({
  title,
  subtitle,
  children,
  table,
  isLoading,
  isEmpty,
  emptyMessage = "No data for this period.",
  error,
  onRetry,
  unavailable,
  actions,
}: ChartCardProps) {
  const [view, setView] = useState<"chart" | "table">("chart");
  const headingId = useId();
  const showToggle = !!table && !isLoading && !error && !isEmpty && !unavailable;

  let content: ReactNode;
  if (isLoading) {
    content = (
      <div role="status" aria-label={`Loading ${title}`} className="flex h-56 items-end gap-3 px-2">
        {[40, 70, 55, 85, 60, 75].map((height, i) => (
          <div key={i} className="w-full animate-pulse rounded-t bg-slate-200 dark:bg-slate-700" style={{ height: `${height}%` }} aria-hidden="true" />
        ))}
      </div>
    );
  } else if (unavailable) {
    content = <EmptyState variant="unavailable" compact />;
  } else if (error) {
    content = <ErrorState error={error} onRetry={onRetry} compact />;
  } else if (isEmpty) {
    content = <EmptyState variant="empty" compact title="No data yet" description={emptyMessage} />;
  } else {
    content = view === "table" && table ? table : children;
  }

  return (
    <section className="card h-full" aria-labelledby={headingId}>
      <div className="flex flex-wrap items-start justify-between gap-2 px-5 pt-4">
        <div>
          <h3 id={headingId} className="text-sm font-semibold text-slate-800 dark:text-slate-100">
            {title}
          </h3>
          {subtitle && <p className="text-xs text-slate-500 dark:text-slate-400">{subtitle}</p>}
        </div>
        <div className="flex items-center gap-2">
          {actions}
          {showToggle && (
            <div className="inline-flex rounded border border-slate-200 text-xs dark:border-slate-600" role="group" aria-label="View as">
              {(["chart", "table"] as const).map((v) => (
                <button
                  key={v}
                  type="button"
                  aria-pressed={view === v}
                  onClick={() => setView(v)}
                  className={`px-2 py-1 capitalize ${view === v ? "bg-slate-100 font-medium dark:bg-slate-700" : ""}`}
                >
                  {v}
                </button>
              ))}
            </div>
          )}
        </div>
      </div>
      <div className="px-3 pb-3 pt-2">{content}</div>
    </section>
  );
}
