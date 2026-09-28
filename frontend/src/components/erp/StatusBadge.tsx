import { resolveStatus, type StatusTone } from "../../lib/erp/statusRegistry";

const TONE_CLASSES: Record<StatusTone, string> = {
  neutral: "bg-slate-100 text-slate-700 dark:bg-slate-700/60 dark:text-slate-200",
  info: "bg-info/10 text-info dark:bg-info/20",
  success: "bg-success/10 text-success dark:bg-success/20",
  warning: "bg-warning/10 text-[#8a6700] dark:bg-warning/20 dark:text-warning",
  danger: "bg-danger/10 text-danger dark:bg-danger/20",
};

export interface StatusBadgeProps {
  status: string | null | undefined;
  /** Module-specific labels, e.g. { posted: "Booked" }. Tone stays registry-defined. */
  labels?: Record<string, string>;
  className?: string;
}

/** The only way ERP pages render a status. Colors come from lib/erp/statusRegistry. */
export function StatusBadge({ status, labels, className = "" }: StatusBadgeProps) {
  const def = resolveStatus(status, labels);
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap ${TONE_CLASSES[def.tone]} ${className}`}
      data-tone={def.tone}
    >
      <i className={`${def.icon} text-sm leading-none`} aria-hidden="true" />
      {def.label}
    </span>
  );
}
