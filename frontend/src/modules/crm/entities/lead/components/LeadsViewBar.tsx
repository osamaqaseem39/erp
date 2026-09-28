import React from "react";
import { Link, useSearchParams } from "react-router-dom";

const toolBtn =
  "inline-flex items-center gap-1.5 px-2 py-1 text-sm font-medium text-[#323130] bg-transparent hover:bg-[#F3F2F1] rounded-sm transition-colors disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent";

const toolBtnActive =
  "inline-flex items-center gap-1.5 px-2 py-1 text-sm font-medium text-[#0078D4] bg-[#EFF6FC] rounded-sm";

interface LeadsViewBarProps {
  view: "hub" | "list";
  onNew: () => void;
  onDelete?: () => void;
  deleteDisabled?: boolean;
}

export const LeadsViewBar: React.FC<LeadsViewBarProps> = ({
  view,
  onNew,
  onDelete,
  deleteDisabled = true,
}) => {
  const [searchParams] = useSearchParams();
  const selectedId = searchParams.get("id");

  const hubTo = selectedId ? `/crm/leads?id=${selectedId}` : "/crm/leads";
  const listTo = "/crm/leads?view=list";

  return (
    <div className="flex flex-wrap items-center gap-0.5 px-1 py-2 mb-2 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-white dark:bg-[#1f1f1f] rounded-t-sm">
      <Link
        to={hubTo}
        className={view === "hub" ? toolBtnActive : toolBtn}
        title="Focused view"
      >
        <i className="mgc_layout_leftbar_open_line text-base" />
        <span className="hidden sm:inline">Focused view</span>
      </Link>
      <Link to={listTo} className={view === "list" ? toolBtnActive : toolBtn} title="List view">
        <i className="mgc_list_check_2_line text-base" />
        <span className="hidden sm:inline">Show as list</span>
      </Link>

      <span className="w-px h-5 bg-[#EDEBE9] mx-1" aria-hidden />

      <button type="button" onClick={onNew} className={toolBtn}>
        <i className="mgc_add_line text-base" />
        New
      </button>
      <button
        type="button"
        className={toolBtn}
        disabled={deleteDisabled}
        onClick={onDelete}
        title="Delete selected"
      >
        <i className="mgc_delete_line text-base" />
        Delete
      </button>

      <span className="flex-1" />

      <Link
        to="/crm/leads/new"
        className="text-sm text-[#0078D4] hover:underline font-medium px-2 py-1"
      >
        Full form
      </Link>
    </div>
  );
};

export default LeadsViewBar;
