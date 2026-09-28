import React from "react";
import type { SortDirection } from "./sortTableData";

export interface SortableThProps {
  columnKey: string;
  label: React.ReactNode;
  sortKey: string;
  sortDir: SortDirection;
  onSort: (key: string) => void;
  className?: string;
  sortable?: boolean;
  /** D365 list view header styling */
  variant?: "default" | "crm";
}

const crmThClass =
  "text-left text-[11px] font-semibold text-[#605E5C] uppercase tracking-wide px-3 py-2 bg-[#FAF9F8] border-b border-[#EDEBE9] whitespace-nowrap";

const SortableTh: React.FC<SortableThProps> = ({
  columnKey,
  label,
  sortKey,
  sortDir,
  onSort,
  className = "",
  sortable = true,
  variant = "default",
}) => {
  if (!sortable) {
    return (
      <th className={variant === "crm" ? `${crmThClass} ${className}`.trim() : className}>
        {label}
      </th>
    );
  }

  const active = sortKey === columnKey;
  const thClass =
    variant === "crm"
      ? `${crmThClass} ${className}`.trim()
      : className;

  return (
    <th className={thClass}>
      <button
        type="button"
        className={
          variant === "crm"
            ? "inline-flex items-center hover:text-[#0078D4] font-semibold uppercase tracking-wide"
            : "inline-flex items-center gap-1 text-inherit font-semibold hover:text-primary cursor-pointer bg-transparent border-0 p-0"
        }
        onClick={() => onSort(columnKey)}
      >
        {label}
        {active && (
          <i
            className={`mgc_arrow_${sortDir === "asc" ? "up" : "down"}_line ${
              variant === "crm" ? "ms-1 text-[#0078D4]" : "text-primary text-sm"
            }`}
            aria-hidden
          />
        )}
        {!active && variant === "default" && (
          <i className="mgc_arrow_up_down_line text-gray-400 text-sm opacity-60" aria-hidden />
        )}
      </button>
    </th>
  );
};

export default SortableTh;
