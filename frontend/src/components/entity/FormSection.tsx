import React from "react";

export interface FormSectionProps {
  title: string;
  description?: string;
  children: React.ReactNode;
  columns?: 1 | 2;
}

const FormSection: React.FC<FormSectionProps> = ({
  title,
  description,
  children,
  columns = 2,
}) => (
  <section className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900/40 p-5">
    <div className="mb-4 border-b border-gray-100 dark:border-gray-700 pb-3">
      <h5 className="text-base font-semibold text-gray-800 dark:text-gray-100">{title}</h5>
      {description && (
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{description}</p>
      )}
    </div>
    <div
      className={
        columns === 1 ? "grid grid-cols-1 gap-4" : "grid grid-cols-1 sm:grid-cols-2 gap-4"
      }
    >
      {children}
    </div>
  </section>
);

export default FormSection;
