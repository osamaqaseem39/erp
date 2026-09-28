import React, {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useMemo,
  useState,
} from "react";
import { Link } from "react-router-dom";
import { useTableSort, SortableTh } from "../../../../../components";
import { leadService, type LeadDto } from "../services";
import {
  formatLeadDateTime,
  isOpenLead,
  leadDisplayName,
  leadTopicLine,
} from "../leadHubUtils";

const thClass =
  "text-left text-[11px] font-semibold text-[#605E5C] uppercase tracking-wide px-3 py-2 bg-[#FAF9F8] border-b border-[#EDEBE9] whitespace-nowrap";
const tdClass = "px-3 py-2 text-sm text-[#323130] border-b border-[#EDEBE9] align-middle";

export interface LeadListHandle {
  reload: () => Promise<void>;
  deleteSelected: () => Promise<void>;
}

interface LeadListProps {
  onSelectionChange?: (count: number) => void;
}

const LeadList = forwardRef<LeadListHandle, LeadListProps>(function LeadList(
  { onSelectionChange },
  ref
) {
  const [leads, setLeads] = useState<LeadDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewFilter, setViewFilter] = useState<"open" | "all">("open");
  const [keyword, setKeyword] = useState("");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("createdAt", "desc");

  const loadLeads = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await leadService.getAll();
      setLeads(data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.error || e.response?.data?.message || "Failed to load leads");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadLeads();
  }, [loadLeads]);

  useEffect(() => {
    onSelectionChange?.(selected.size);
  }, [selected.size, onSelectionChange]);

  const filteredLeads = useMemo(() => {
    let list = viewFilter === "open" ? leads.filter(isOpenLead) : leads;
    const q = keyword.trim().toLowerCase();
    if (q) {
      list = list.filter((l) => {
        const hay = [
          l.firstName,
          l.lastName,
          l.companyName,
          l.email,
          l.leadCode,
          l.notes,
          l.status,
        ]
          .filter(Boolean)
          .join(" ")
          .toLowerCase();
        return hay.includes(q);
      });
    }
    return list;
  }, [leads, viewFilter, keyword]);

  const sortedLeads = useMemo(
    () =>
      sortRows(filteredLeads, (row, key) => {
        if (key === "name") return leadDisplayName(row);
        if (key === "topic") return leadTopicLine(row);
        return (row as Record<string, unknown>)[key];
      }),
    [filteredLeads, sortRows]
  );

  const allSelected =
    filteredLeads.length > 0 && filteredLeads.every((l) => l.id && selected.has(l.id));

  const toggleAll = () => {
    if (allSelected) {
      setSelected(new Set());
    } else {
      setSelected(new Set(filteredLeads.map((l) => l.id).filter(Boolean) as string[]));
    }
  };

  const toggleRow = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const deleteSelected = async () => {
    if (selected.size === 0) return;
    if (!window.confirm(`Delete ${selected.size} selected lead(s)?`)) return;
    try {
      await Promise.all([...selected].map((id) => leadService.delete(id)));
      setSelected(new Set());
      await loadLeads();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to delete leads");
    }
  };

  useImperativeHandle(ref, () => ({
    reload: loadLeads,
    deleteSelected,
  }));

  return (
    <div className="flex flex-col min-h-[560px]">
      <div className="flex flex-wrap items-center gap-3 px-4 py-3 border-b border-[#EDEBE9] bg-[#FAF9F8]">
        <select
          value={viewFilter}
          onChange={(e) => setViewFilter(e.target.value as "open" | "all")}
          className="text-base font-semibold text-[#323130] bg-transparent border-0 p-0 pe-6 cursor-pointer focus:ring-0"
        >
          <option value="open">My Open Leads</option>
          <option value="all">All Leads</option>
        </select>

        <button
          type="button"
          className="text-sm text-[#605E5C] hover:text-[#323130] px-2 py-1 rounded-sm hover:bg-[#F3F2F1]"
          title="Column layout (coming soon)"
        >
          <i className="mgc_settings_3_line me-1" />
          Edit columns
        </button>
        <button
          type="button"
          className="text-sm text-[#605E5C] hover:text-[#323130] px-2 py-1 rounded-sm hover:bg-[#F3F2F1]"
          title="Advanced filters (coming soon)"
        >
          <i className="mgc_filter_line me-1" />
          Edit filters
        </button>

        <div className="relative ms-auto min-w-[200px] max-w-sm flex-1">
          <i className="mgc_search_line absolute left-2.5 top-1/2 -translate-y-1/2 text-[#8A8886]" />
          <input
            type="search"
            placeholder="Filter by keyword"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            className="w-full pl-8 pr-3 py-1.5 text-sm border border-[#8A8886] rounded-sm bg-white focus:outline-none focus:border-[#0078D4] focus:ring-1 focus:ring-[#0078D4]"
          />
        </div>

        <button
          type="button"
          onClick={() => void loadLeads()}
          className="p-1.5 text-[#605E5C] hover:text-[#0078D4] rounded-sm hover:bg-[#F3F2F1]"
          title="Refresh"
        >
          <i className="mgc_refresh_1_line text-lg" />
        </button>
      </div>

      {error && (
        <div className="mx-4 mt-3 px-4 py-2 rounded-sm border border-[#F1BBBC] bg-[#FDE7E9] text-[#A4262C] text-sm">
          {error}
        </div>
      )}

      <div className="flex-1 overflow-auto">
        {loading ? (
          <p className="text-center text-sm text-[#605E5C] py-16">Loading…</p>
        ) : (
          <table className="w-full mb-0 border-collapse">
            <thead className="sticky top-0 z-[1]">
              <tr>
                <th className={`${thClass} w-10`}>
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={toggleAll}
                    aria-label="Select all"
                    className="rounded border-[#8A8886] text-[#0078D4] focus:ring-[#0078D4]"
                  />
                </th>
                <SortableTh variant="crm" columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh variant="crm" columnKey="topic" label="Topic" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh variant="crm" columnKey="status" label="Status Reason" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh variant="crm" columnKey="createdAt" label="Created On" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
              </tr>
            </thead>
            <tbody>
              {filteredLeads.length === 0 ? (
                <tr>
                  <td colSpan={5} className="text-center text-[#605E5C] py-16 text-sm">
                    No leads found
                  </td>
                </tr>
              ) : (
                sortedLeads.map((lead) => {
                  const id = lead.id || "";
                  const checked = selected.has(id);
                  return (
                    <tr
                      key={id}
                      className={`hover:bg-[#F3F2F1] ${checked ? "bg-[#EFF6FC]" : ""}`}
                    >
                      <td className={tdClass}>
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() => toggleRow(id)}
                          aria-label={`Select ${leadDisplayName(lead)}`}
                          className="rounded border-[#8A8886] text-[#0078D4] focus:ring-[#0078D4]"
                        />
                      </td>
                      <td className={tdClass}>
                        <Link
                          to={`/crm/leads?id=${id}`}
                          className="font-medium text-[#0078D4] hover:underline"
                        >
                          {leadDisplayName(lead)}
                        </Link>
                      </td>
                      <td className={`${tdClass} text-[#605E5C] max-w-md truncate`} title={leadTopicLine(lead)}>
                        {leadTopicLine(lead)}
                      </td>
                      <td className={tdClass}>{lead.status || "New"}</td>
                      <td className={`${tdClass} text-[#605E5C] whitespace-nowrap`}>
                        {formatLeadDateTime(lead.createdAt)}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        )}
      </div>

      <div className="px-4 py-2 border-t border-[#EDEBE9] text-sm text-[#605E5C] bg-[#FAF9F8]">
        Rows: {filteredLeads.length}
        {selected.size > 0 && (
          <span className="ms-3 text-[#0078D4]">{selected.size} selected</span>
        )}
      </div>
    </div>
  );
});

export default LeadList;
