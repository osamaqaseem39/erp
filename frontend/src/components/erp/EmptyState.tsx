import type { ReactNode } from "react";

export type EmptyStateVariant = "empty" | "no-results" | "unavailable" | "error" | "forbidden";

const VARIANT_ICON: Record<EmptyStateVariant, string> = {
  empty: "mgc_inbox_line",
  "no-results": "mgc_search_line",
  unavailable: "mgc_time_line",
  error: "mgc_warning_line",
  forbidden: "mgc_lock_line",
};

const VARIANT_DEFAULTS: Record<EmptyStateVariant, { title: string; description: string }> = {
  empty: { title: "Nothing here yet", description: "Records you add will appear here." },
  "no-results": { title: "No matching records", description: "Try a different search or clear the filters." },
  unavailable: {
    title: "Not available yet",
    description: "This part of the system is still being built, so no data is shown here yet.",
  },
  error: { title: "Unable to load", description: "Something went wrong while loading this data." },
  forbidden: { title: "You don't have access", description: "Ask an administrator for permission if you need this." },
};

export interface EmptyStateProps {
  variant?: EmptyStateVariant;
  title?: string;
  description?: ReactNode;
  /** Primary next step, e.g. an "Add customer" button. */
  action?: ReactNode;
  compact?: boolean;
  className?: string;
}

export function EmptyState({ variant = "empty", title, description, action, compact = false, className = "" }: EmptyStateProps) {
  const defaults = VARIANT_DEFAULTS[variant];
  const isAlert = variant === "error";
  return (
    <div
      className={`flex flex-col items-center justify-center text-center ${compact ? "py-6 px-4" : "py-12 px-6"} ${className}`}
      role={isAlert ? "alert" : "status"}
      data-variant={variant}
    >
      <span
        className={`mb-3 inline-flex items-center justify-center rounded-full ${compact ? "h-10 w-10 text-xl" : "h-14 w-14 text-2xl"} ${
          isAlert ? "bg-danger/10 text-danger" : "bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-300"
        }`}
        aria-hidden="true"
      >
        <i className={VARIANT_ICON[variant]} />
      </span>
      <h3 className={`font-semibold text-slate-800 dark:text-slate-100 ${compact ? "text-sm" : "text-base"}`}>
        {title ?? defaults.title}
      </h3>
      <div className="mt-1 max-w-md text-sm text-slate-500 dark:text-slate-400">{description ?? defaults.description}</div>
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}
