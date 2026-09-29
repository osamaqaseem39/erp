/**
 * One meaning per status across the whole ERP. Pages never pick badge colors themselves:
 * they pass the raw status string to <StatusBadge>, which reads this registry.
 */

export type StatusTone = "neutral" | "info" | "success" | "warning" | "danger";

export interface StatusDefinition {
  label: string;
  tone: StatusTone;
  /** Mingcute icon class, so status is never conveyed by color alone. */
  icon: string;
}

const TONE_ICONS: Record<StatusTone, string> = {
  neutral: "mgc_minus_circle_line",
  info: "mgc_time_line",
  success: "mgc_check_circle_line",
  warning: "mgc_alert_line",
  danger: "mgc_close_circle_line",
};

const TONE_BY_STATUS: Record<string, StatusTone> = {
  // neutral: not started, parked or retired
  draft: "neutral",
  new: "neutral",
  inactive: "neutral",
  archived: "neutral",
  closed: "neutral",
  planned: "neutral",
  // info: moving through a process
  open: "info",
  sent: "info",
  submitted: "info",
  inprogress: "info",
  released: "info",
  intransit: "info",
  partiallypaid: "info",
  partiallyreceived: "info",
  qualified: "info",
  proposal: "info",
  negotiation: "info",
  qualitycheck: "info",
  shipped: "info",
  // warning: waiting on someone or at risk
  pending: "warning",
  pendingapproval: "warning",
  onhold: "warning",
  onleave: "warning",
  suspended: "warning",
  expiring: "warning",
  // success: done or healthy
  active: "success",
  approved: "success",
  completed: "success",
  paid: "success",
  posted: "success",
  won: "success",
  received: "success",
  converted: "success",
  invoiced: "success",
  // danger: failed, stopped or overdue
  rejected: "danger",
  cancelled: "danger",
  canceled: "danger",
  failed: "danger",
  overdue: "danger",
  terminated: "danger",
  lost: "danger",
  void: "danger",
  voided: "danger",
  expired: "danger",
  locked: "danger",
  // log levels
  error: "danger",
  warning: "warning",
  information: "info",
  info: "info",
  debug: "neutral",
};

const normalize = (status: string) => status.toLowerCase().replace(/[\s_-]+/g, "");

/** "PendingApproval" / "pending_approval" / "pending approval" -> "Pending approval" */
export function humanizeStatus(status: string): string {
  const words = status
    .replace(/[_-]+/g, " ")
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .trim()
    .toLowerCase();
  return words ? words[0].toUpperCase() + words.slice(1) : "";
}

const warned = new Set<string>();

/**
 * Resolves a status to its label, tone and icon. `overrides` lets a module relabel a status
 * (e.g. "Posted" -> "Booked") but not change its tone, so colors stay consistent everywhere.
 */
export function resolveStatus(status: string | null | undefined, overrides?: Record<string, string>): StatusDefinition {
  const raw = (status ?? "").trim();
  if (!raw) return { label: "—", tone: "neutral", icon: TONE_ICONS.neutral };

  const key = normalize(raw);
  const tone = TONE_BY_STATUS[key];
  if (!tone && import.meta.env.DEV && !warned.has(key)) {
    warned.add(key);
    console.warn(`[statusRegistry] Unknown status "${raw}" rendered as neutral. Add it to TONE_BY_STATUS.`);
  }

  const resolvedTone = tone ?? "neutral";
  const overrideKey = overrides && Object.keys(overrides).find((k) => normalize(k) === key);
  return {
    label: overrideKey ? overrides[overrideKey] : humanizeStatus(raw),
    tone: resolvedTone,
    icon: TONE_ICONS[resolvedTone],
  };
}

/** Boolean active flags (isActive) map onto the same vocabulary. */
export const activeStatus = (isActive: boolean | null | undefined) => (isActive ? "Active" : "Inactive");
