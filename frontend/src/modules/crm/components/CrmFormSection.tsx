import React from "react";

export interface CrmFormSectionProps {
  title: string;
  description?: string;
  children: React.ReactNode;
  columns?: 1 | 2;
}

/** @deprecated Legacy layout kept until its module is migrated. Use FormSection from components/erp. */
const CrmFormSection: React.FC<CrmFormSectionProps> = ({
  title,
  description,
  children,
  columns = 2,
}) => (
  <section className="crm-form-section border border-[#EDEBE9] dark:border-[#3b3a39] rounded-sm bg-white dark:bg-[#1f1f1f] overflow-hidden">
    <header className="px-4 py-3 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-[#FAF9F8] dark:bg-[#292827]">
      <h3 className="text-[15px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0">{title}</h3>
      {description && (
        <p className="text-xs text-[#605E5C] dark:text-[#a19f9d] mt-0.5 mb-0">{description}</p>
      )}
    </header>
    <div
      className={
        columns === 1
          ? "crm-form-section-body crm-form-section-body--single px-4 py-2"
          : "crm-form-section-body crm-form-section-body--two-col px-4 py-2"
      }
    >
      {children}
    </div>
  </section>
);

export default CrmFormSection;
