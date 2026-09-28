import React, { Fragment, useEffect, useState } from "react";
import { Menu, Transition } from "@headlessui/react";
import { OffcanvasLayout } from "../../../../../components/HeadlessUI";
import { leadService } from "../services";
import type { CreateLeadDto } from "../types";
import { LEAD_SOURCE_OPTIONS } from "../types";
import { D365 } from "../../../theme/crmDynamicsTheme";

const PURCHASE_TIMEFRAME_OPTIONS = [
  { value: "", label: "—" },
  { value: "Immediate", label: "Immediate" },
  { value: "This Quarter", label: "This quarter" },
  { value: "This Year", label: "This year" },
  { value: "Unknown", label: "Unknown" },
];

const inputClass =
  "w-full px-2 py-1.5 text-sm text-[#323130] bg-[#F3F2F1] border border-transparent rounded-sm focus:outline-none focus:bg-white focus:border-[#0078D4] focus:ring-1 focus:ring-[#0078D4] placeholder:text-[#8A8886]";

const selectClass = `${inputClass} cursor-pointer`;

interface FormState {
  topic: string;
  estimatedValue: string;
  purchaseTimeframe: string;
  leadSource: string;
  firstName: string;
  lastName: string;
  jobTitle: string;
  companyName: string;
  email: string;
  phone: string;
  description: string;
}

const emptyForm: FormState = {
  topic: "",
  estimatedValue: "",
  purchaseTimeframe: "",
  leadSource: "",
  firstName: "",
  lastName: "",
  jobTitle: "",
  companyName: "",
  email: "",
  phone: "",
  description: "",
};

function generateLeadCode(): string {
  return `LD-QC${Date.now().toString(36).toUpperCase().slice(-8)}`;
}

function buildNotes(form: FormState): string | undefined {
  const parts: string[] = [];
  if (form.topic.trim()) parts.push(form.topic.trim());
  if (form.purchaseTimeframe) parts.push(`Purchase timeframe: ${form.purchaseTimeframe}`);
  if (form.description.trim()) parts.push(form.description.trim());
  return parts.length > 0 ? parts.join("\n\n") : undefined;
}

interface QuickFieldProps {
  label: string;
  required?: boolean;
  hint?: string;
  children: React.ReactNode;
}

const QuickField: React.FC<QuickFieldProps> = ({ label, required, hint, children }) => (
  <div className="grid grid-cols-[minmax(0,38%)_1fr] gap-x-3 gap-y-0.5 items-start py-2">
    <label className="text-sm text-[#323130] pt-1.5 leading-snug">
      {label}
      {required && <span className="text-[#A4262C] ms-0.5">*</span>}
      {hint && (
        <span className="ms-1 text-[#0078D4] text-xs font-semibold" title={hint}>
          +
        </span>
      )}
    </label>
    <div>{children}</div>
  </div>
);

interface LeadQuickCreatePanelProps {
  open: boolean;
  onClose: () => void;
  onCreated?: (leadId: string) => void;
}

export const LeadQuickCreatePanel: React.FC<LeadQuickCreatePanelProps> = ({
  open,
  onClose,
  onCreated,
}) => {
  const [form, setForm] = useState<FormState>(emptyForm);
  const [errors, setErrors] = useState<Partial<Record<keyof FormState, string>>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) {
      setForm(emptyForm);
      setErrors({});
      setSubmitError(null);
    }
  }, [open]);

  const set =
    (field: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
      setErrors((prev) => ({ ...prev, [field]: undefined }));
      setSubmitError(null);
    };

  const validate = (): boolean => {
    const next: Partial<Record<keyof FormState, string>> = {};
    if (!form.topic.trim()) next.topic = "Topic is required";
    if (!form.firstName.trim()) next.firstName = "First name is required";
    if (!form.lastName.trim()) next.lastName = "Last name is required";
    if (!form.leadSource) next.leadSource = "Lead source is required";
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      next.email = "Enter a valid email";
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const buildPayload = (): CreateLeadDto => {
    const estimated = form.estimatedValue.trim() ? parseFloat(form.estimatedValue) : undefined;
    return {
      leadCode: generateLeadCode(),
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      companyName: form.companyName.trim() || undefined,
      email: form.email.trim() || undefined,
      phone: form.phone.trim() || undefined,
      contactPerson: form.jobTitle.trim() || undefined,
      leadSource: form.leadSource,
      source: form.leadSource,
      status: "New",
      priority: "Medium",
      estimatedValue: estimated != null && !Number.isNaN(estimated) ? estimated : undefined,
      notes: buildNotes(form),
    };
  };

  const submit = async (mode: "close" | "new") => {
    if (!validate()) return;
    setSaving(true);
    setSubmitError(null);
    try {
      const id = await leadService.create(buildPayload());
      onCreated?.(id);
      if (mode === "close") {
        onClose();
      } else {
        setForm(emptyForm);
        setErrors({});
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; error?: string; message?: string } } };
      setSubmitError(
        e.response?.data?.Error ||
          e.response?.data?.error ||
          e.response?.data?.message ||
          "Failed to create lead"
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <OffcanvasLayout
      open={open}
      toggleOffcanvas={onClose}
      placement="end"
      sizeClassName="!w-full max-w-[420px]"
    >
      <div
        className="flex flex-col h-full bg-white font-['Segoe_UI','Segoe_UI_Web',system-ui,sans-serif]"
        style={{ color: D365.text }}
      >
        <header className="flex items-center justify-between shrink-0 px-4 py-3 border-b border-[#EDEBE9]">
          <h2 className="text-base font-semibold text-[#323130] mb-0">Quick Create: Lead</h2>
          <button
            type="button"
            onClick={onClose}
            className="flex h-8 w-8 items-center justify-center text-[#605E5C] hover:bg-[#F3F2F1] rounded-sm"
            aria-label="Close"
          >
            <i className="mgc_close_line text-lg" />
          </button>
        </header>

        <div className="flex-1 overflow-y-auto px-4 py-2">
          {submitError && (
            <div className="mb-3 px-3 py-2 text-sm rounded-sm border border-[#F1BBBC] bg-[#FDE7E9] text-[#A4262C]">
              {submitError}
            </div>
          )}

          <section className="mb-4">
            <h3 className="text-sm font-semibold text-[#323130] mb-2 pb-1 border-b border-[#EDEBE9]">
              Details
            </h3>
            <button
              type="button"
              disabled
              title="Coming soon"
              className="w-full mb-3 inline-flex items-center justify-center gap-2 px-3 py-2 text-sm font-medium text-white bg-[#0078D4] opacity-60 cursor-not-allowed rounded-sm"
            >
              <i className="mgc_upload_2_line" />
              Scan business card
            </button>
            <QuickField label="Topic" required>
              <input
                type="text"
                className={inputClass}
                placeholder="—"
                value={form.topic}
                onChange={set("topic")}
                aria-invalid={!!errors.topic}
              />
              {errors.topic && <p className="text-xs text-[#A4262C] mt-0.5 mb-0">{errors.topic}</p>}
            </QuickField>
            <QuickField label="Budget Amount">
              <input
                type="number"
                min="0"
                step="0.01"
                className={inputClass}
                placeholder="—"
                value={form.estimatedValue}
                onChange={set("estimatedValue")}
              />
            </QuickField>
            <QuickField label="Purchase Timeframe">
              <select className={selectClass} value={form.purchaseTimeframe} onChange={set("purchaseTimeframe")}>
                {PURCHASE_TIMEFRAME_OPTIONS.map((o) => (
                  <option key={o.value || "empty"} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </QuickField>
            <QuickField label="Lead Source" required>
              <select
                className={selectClass}
                value={form.leadSource}
                onChange={set("leadSource")}
                aria-invalid={!!errors.leadSource}
              >
                <option value="">—</option>
                {LEAD_SOURCE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
              {errors.leadSource && (
                <p className="text-xs text-[#A4262C] mt-0.5 mb-0">{errors.leadSource}</p>
              )}
            </QuickField>
          </section>

          <section className="mb-4">
            <h3 className="text-sm font-semibold text-[#323130] mb-2 pb-1 border-b border-[#EDEBE9]">
              Personal Information
            </h3>
            <QuickField label="First Name" required hint="Add related contact">
              <input
                type="text"
                className={inputClass}
                placeholder="—"
                value={form.firstName}
                onChange={set("firstName")}
                aria-invalid={!!errors.firstName}
              />
              {errors.firstName && (
                <p className="text-xs text-[#A4262C] mt-0.5 mb-0">{errors.firstName}</p>
              )}
            </QuickField>
            <QuickField label="Last Name" required>
              <input
                type="text"
                className={inputClass}
                placeholder="—"
                value={form.lastName}
                onChange={set("lastName")}
                aria-invalid={!!errors.lastName}
              />
              {errors.lastName && (
                <p className="text-xs text-[#A4262C] mt-0.5 mb-0">{errors.lastName}</p>
              )}
            </QuickField>
            <QuickField label="Job Title">
              <input
                type="text"
                className={inputClass}
                placeholder="—"
                value={form.jobTitle}
                onChange={set("jobTitle")}
              />
            </QuickField>
            <QuickField label="Company Name" hint="Add account">
              <input
                type="text"
                className={inputClass}
                placeholder="—"
                value={form.companyName}
                onChange={set("companyName")}
              />
            </QuickField>
          </section>

          <section className="mb-2">
            <h3 className="text-sm font-semibold text-[#323130] mb-2 pb-1 border-b border-[#EDEBE9]">
              Contact Details
            </h3>
            <QuickField label="Email">
              <input
                type="email"
                className={inputClass}
                placeholder="—"
                value={form.email}
                onChange={set("email")}
                aria-invalid={!!errors.email}
              />
              {errors.email && <p className="text-xs text-[#A4262C] mt-0.5 mb-0">{errors.email}</p>}
            </QuickField>
            <QuickField label="Mobile Phone">
              <input
                type="tel"
                className={inputClass}
                placeholder="—"
                value={form.phone}
                onChange={set("phone")}
              />
            </QuickField>
            <QuickField label="Description">
              <textarea
                rows={3}
                className={`${inputClass} resize-y min-h-[72px]`}
                placeholder="—"
                value={form.description}
                onChange={set("description")}
              />
            </QuickField>
          </section>
        </div>

        <footer className="shrink-0 flex items-center justify-end gap-2 px-4 py-3 border-t border-[#EDEBE9] bg-[#FAF9F8]">
          <button
            type="button"
            onClick={onClose}
            disabled={saving}
            className="px-4 py-1.5 text-sm font-medium text-[#323130] bg-white border border-[#8A8886] hover:bg-[#F3F2F1] rounded-sm disabled:opacity-50"
          >
            Cancel
          </button>
          <Menu as="div" className="relative inline-flex">
            <button
              type="button"
              disabled={saving}
              onClick={() => void submit("close")}
              className="inline-flex items-center rounded-l-sm px-4 py-1.5 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] disabled:opacity-50 border-r border-[#005A9E]"
            >
              {saving ? "Saving…" : "Save"}
            </button>
            <Menu.Button
              disabled={saving}
              className="inline-flex items-center rounded-r-sm px-2 py-1.5 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] disabled:opacity-50"
              aria-label="More save options"
            >
              <i className="mgc_down_line text-base" />
            </Menu.Button>
            <Transition
              as={Fragment}
              enter="transition ease-out duration-100"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="transition ease-in duration-75"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Menu.Items className="absolute bottom-full right-0 mb-1 w-44 origin-bottom-right rounded-sm bg-white shadow-lg ring-1 ring-[#EDEBE9] focus:outline-none z-10">
                <Menu.Item>
                  {({ active }) => (
                    <button
                      type="button"
                      className={`${active ? "bg-[#F3F2F1]" : ""} block w-full text-left px-3 py-2 text-sm text-[#323130]`}
                      onClick={() => void submit("close")}
                    >
                      Save and close
                    </button>
                  )}
                </Menu.Item>
                <Menu.Item>
                  {({ active }) => (
                    <button
                      type="button"
                      className={`${active ? "bg-[#F3F2F1]" : ""} block w-full text-left px-3 py-2 text-sm text-[#323130]`}
                      onClick={() => void submit("new")}
                    >
                      Save and create new
                    </button>
                  )}
                </Menu.Item>
              </Menu.Items>
            </Transition>
          </Menu>
        </footer>
      </div>
    </OffcanvasLayout>
  );
};

export default LeadQuickCreatePanel;
