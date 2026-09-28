import type { LeadDto } from "./types";

export const OPEN_LEAD_STATUSES = ["New", "Contacted", "Qualified", "Nurturing"];

export const LEAD_BPF_STAGES = [
  { id: "qualify", label: "Qualify", statuses: ["New", "Contacted"] },
  { id: "develop", label: "Develop", statuses: ["Nurturing"] },
  { id: "propose", label: "Propose", statuses: ["Qualified"] },
  { id: "close", label: "Close", statuses: ["Converted", "Lost"] },
] as const;

export interface BpfStageField {
  label: string;
  value: string;
}

function bpfField(label: string, value?: string | number | null): BpfStageField | null {
  if (value == null || value === "") return null;
  return { label, value: String(value) };
}

export function priorityToRating(priority?: string): string {
  switch (priority) {
    case "Urgent":
    case "High":
      return "Hot";
    case "Low":
      return "Cold";
    default:
      return "Warm";
  }
}

export function getActiveBpfStageIndex(status?: string): number {
  const current = status || "New";
  const idx = LEAD_BPF_STAGES.findIndex((s) =>
    (s.statuses as readonly string[]).includes(current)
  );
  return idx >= 0 ? idx : 0;
}

export function formatLeadDate(value?: string): string {
  if (!value) return "—";
  return new Date(value).toLocaleDateString(undefined, {
    month: "numeric",
    day: "numeric",
    year: "numeric",
  });
}

export function formatLeadDateTime(value?: string): string {
  if (!value) return "—";
  return new Date(value).toLocaleString(undefined, {
    month: "numeric",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

export function formatLeadCurrency(amount?: number): string {
  if (amount == null) return "";
  return new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(amount);
}

export function leadDisplayName(lead: LeadDto): string {
  return `${lead.firstName} ${lead.lastName}`.trim();
}

export function leadInitials(lead: LeadDto): string {
  const a = lead.firstName?.[0] ?? "";
  const b = lead.lastName?.[0] ?? "";
  return (a + b).toUpperCase() || "?";
}

export function ownerInitials(name?: string): string {
  if (!name?.trim()) return "?";
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
}

export function leadTopicValue(lead: LeadDto): string {
  if (!lead.notes?.trim()) return leadTopicLine(lead) === "No description" ? "" : leadTopicLine(lead);
  const first = lead.notes.trim().split("\n")[0];
  return first;
}

export function leadTopicLine(lead: LeadDto): string {
  if (lead.notes?.trim()) {
    const trimmed = lead.notes.trim();
    return trimmed.length > 72 ? `${trimmed.slice(0, 72)}…` : trimmed;
  }
  if (lead.estimatedValue != null && lead.companyName) {
    return `${formatLeadCurrency(lead.estimatedValue)} for ${lead.companyName}`;
  }
  return lead.companyName || lead.email || "No description";
}

export function leadSummaryText(lead: LeadDto): string {
  const parts: string[] = [];
  const topic = leadTopicLine(lead);
  if (topic && topic !== "No description") {
    parts.push(`This lead is interested in ${topic}.`);
  }
  if (lead.estimatedValue != null) {
    parts.push(`Estimated deal size is ${formatLeadCurrency(lead.estimatedValue)}.`);
  }
  if (lead.companyName) {
    parts.push(`Company: ${lead.companyName}.`);
  }
  if (lead.industry) {
    parts.push(`Industry: ${lead.industry}.`);
  }
  return parts.length > 0
    ? parts.join(" ")
    : "Review lead details and schedule the next activity to move this opportunity forward.";
}

export function isOpenLead(lead: LeadDto): boolean {
  return OPEN_LEAD_STATUSES.includes(lead.status || "New");
}

export function daysInStage(lead: LeadDto): number {
  const ref = lead.updatedAt || lead.createdAt;
  if (!ref) return 0;
  const diff = Date.now() - new Date(ref).getTime();
  return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)));
}

/** Field values captured at a specific BPF stage (shown on past/active tiles). */
export function getBpfStageFields(lead: LeadDto, stageIndex: number): BpfStageField[] {
  const stage = LEAD_BPF_STAGES[stageIndex];
  if (!stage) return [];

  const contactName = leadDisplayName(lead);
  const source = lead.leadSource || lead.source;

  switch (stage.id) {
    case "qualify":
      return [
        bpfField("Contact", contactName),
        bpfField("Company", lead.companyName),
        bpfField("Email", lead.email),
        bpfField("Phone", lead.phone),
        bpfField("Source", source),
      ].filter((f): f is BpfStageField => f != null);
    case "develop":
      return [
        bpfField("Topic", leadTopicValue(lead) || undefined),
        bpfField("Est. value", lead.estimatedValue != null ? formatLeadCurrency(lead.estimatedValue) : undefined),
        bpfField("Last contact", lead.lastContactDate ? formatLeadDate(lead.lastContactDate) : undefined),
        bpfField("Score", lead.leadScore),
      ].filter((f): f is BpfStageField => f != null);
    case "propose":
      return [
        bpfField("Company", lead.companyName),
        bpfField("Industry", lead.industry),
        bpfField("Est. value", lead.estimatedValue != null ? formatLeadCurrency(lead.estimatedValue) : undefined),
        bpfField("Rating", priorityToRating(lead.priority)),
        bpfField("Priority", lead.priority),
      ].filter((f): f is BpfStageField => f != null);
    case "close":
      return [
        bpfField("Outcome", lead.status),
        bpfField("Converted", lead.convertedDate ? formatLeadDate(lead.convertedDate) : undefined),
      ].filter((f): f is BpfStageField => f != null);
    default:
      return [];
  }
}

/** Summary lines from all completed stages (shown on the active stage). */
export function getBpfPriorStageSummary(lead: LeadDto, activeIndex: number): BpfStageField[] {
  if (activeIndex <= 0) return [];
  const seen = new Set<string>();
  const summary: BpfStageField[] = [];
  for (let i = 0; i < activeIndex; i++) {
    for (const field of getBpfStageFields(lead, i)) {
      const key = `${field.label}:${field.value}`;
      if (!seen.has(key)) {
        seen.add(key);
        summary.push(field);
      }
    }
  }
  return summary.slice(0, 4);
}
