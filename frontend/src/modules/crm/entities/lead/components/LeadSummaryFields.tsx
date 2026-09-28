import React, { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import type { LeadDto, UpdateLeadDto } from "../types";
import { leadTopicValue } from "../leadHubUtils";

const fieldInput =
  "w-full text-sm text-[#323130] bg-transparent border-0 border-b border-[#EDEBE9] rounded-none px-0 py-1.5 focus:outline-none focus:border-[#0078D4] focus:ring-0 placeholder:text-[#C8C6C4]";

interface FieldRowProps {
  label: string;
  required?: boolean;
  children: React.ReactNode;
  action?: React.ReactNode;
}

const FieldRow: React.FC<FieldRowProps> = ({ label, required, children, action }) => (
  <div className="grid grid-cols-[minmax(0,38%)_1fr] gap-x-4 gap-y-1 py-2.5 border-b border-[#EDEBE9] items-start">
    <label className="text-sm text-[#323130] pt-1.5">
      {label}
      {required && <span className="text-[#A4262C] ms-0.5">*</span>}
    </label>
    <div className="flex items-center gap-2 min-w-0">
      <div className="flex-1 min-w-0">{children}</div>
      {action}
    </div>
  </div>
);

export interface LeadSummaryFieldsHandle {
  save: () => Promise<boolean>;
}

interface LeadSummaryFieldsProps {
  lead: LeadDto;
  onSave: (data: UpdateLeadDto) => Promise<void>;
}

interface FormState {
  topic: string;
  firstName: string;
  lastName: string;
  jobTitle: string;
  phone: string;
  email: string;
  companyName: string;
  industry: string;
  address: string;
  city: string;
}

const LeadSummaryFields = forwardRef<LeadSummaryFieldsHandle, LeadSummaryFieldsProps>(
  function LeadSummaryFields({ lead, onSave }, ref) {
    const [form, setForm] = useState<FormState>({
      topic: "",
      firstName: "",
      lastName: "",
      jobTitle: "",
      phone: "",
      email: "",
      companyName: "",
      industry: "",
      address: "",
      city: "",
    });

    useEffect(() => {
      setForm({
        topic: leadTopicValue(lead),
        firstName: lead.firstName ?? "",
        lastName: lead.lastName ?? "",
        jobTitle: lead.contactPerson ?? "",
        phone: lead.phone ?? "",
        email: lead.email ?? "",
        companyName: lead.companyName ?? "",
        industry: lead.industry ?? "",
        address: lead.address ?? "",
        city: lead.city ?? "",
      });
    }, [lead.id, lead.updatedAt]);

    const buildPayload = (): UpdateLeadDto | null => {
      if (!lead.id) return null;
      const extraNote = lead.notes?.includes("\n") ? lead.notes.split("\n").slice(1).join("\n") : "";
      const notes = [form.topic.trim(), extraNote.trim()].filter(Boolean).join("\n\n") || undefined;
      return {
        ...lead,
        id: lead.id,
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        contactPerson: form.jobTitle.trim() || undefined,
        phone: form.phone.trim() || undefined,
        email: form.email.trim() || undefined,
        companyName: form.companyName.trim() || undefined,
        industry: form.industry.trim() || undefined,
        address: form.address.trim() || undefined,
        city: form.city.trim() || undefined,
        notes,
      };
    };

    const save = async (): Promise<boolean> => {
      const payload = buildPayload();
      if (!payload || !form.firstName.trim() || !form.lastName.trim()) return false;
      await onSave(payload);
      return true;
    };

    useImperativeHandle(ref, () => ({ save }));

    const set =
      (key: keyof FormState) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setForm((prev) => ({ ...prev, [key]: e.target.value }));
      };

    return (
      <div className="px-1">
        <FieldRow label="Topic" required>
          <input type="text" className={fieldInput} value={form.topic} onChange={set("topic")} />
        </FieldRow>
        <FieldRow label="First Name" required>
          <input type="text" className={fieldInput} value={form.firstName} onChange={set("firstName")} />
        </FieldRow>
        <FieldRow label="Last Name" required>
          <input type="text" className={fieldInput} value={form.lastName} onChange={set("lastName")} />
        </FieldRow>
        <FieldRow label="Job Title">
          <input type="text" className={fieldInput} value={form.jobTitle} onChange={set("jobTitle")} />
        </FieldRow>
        <FieldRow
          label="Business Phone"
          action={
            form.phone ? (
              <a href={`tel:${form.phone}`} className="text-[#0078D4] shrink-0" title="Call">
                <i className="mgc_phone_line text-lg" />
              </a>
            ) : null
          }
        >
          <input type="tel" className={fieldInput} value={form.phone} onChange={set("phone")} />
        </FieldRow>
        <FieldRow
          label="Email"
          action={
            form.email ? (
              <a href={`mailto:${form.email}`} className="text-[#0078D4] shrink-0" title="Email">
                <i className="mgc_mail_line text-lg" />
              </a>
            ) : null
          }
        >
          <input type="email" className={fieldInput} value={form.email} onChange={set("email")} />
        </FieldRow>

        <p className="text-[11px] font-semibold text-[#605E5C] uppercase tracking-wide mt-5 mb-2 pt-2 border-t border-[#EDEBE9]">
          Company
        </p>
        <FieldRow label="Company Name">
          <input type="text" className={fieldInput} value={form.companyName} onChange={set("companyName")} />
        </FieldRow>
        <FieldRow label="Industry">
          <input type="text" className={fieldInput} value={form.industry} onChange={set("industry")} />
        </FieldRow>
        <FieldRow label="Address">
          <input type="text" className={fieldInput} value={form.address} onChange={set("address")} />
        </FieldRow>
        <FieldRow label="City">
          <input type="text" className={fieldInput} value={form.city} onChange={set("city")} />
        </FieldRow>
      </div>
    );
  }
);

export default LeadSummaryFields;
