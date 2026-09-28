import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Tab } from "@headlessui/react";
import SimpleBar from "simplebar-react";
import "simplebar-react/dist/simplebar.min.css";
import { leadService } from "../services";
import type { LeadDto } from "../types";
import { activityService } from "../../activity/services";
import type { ActivityDto } from "../../activity/types";
import { LeadBusinessProcessFlow } from "../components/LeadBusinessProcessFlow";
import { LeadDetailHeader } from "../components/LeadDetailHeader";
import LeadSummaryFields, { type LeadSummaryFieldsHandle } from "../components/LeadSummaryFields";
import { LeadUpNextPanel } from "../components/LeadUpNextPanel";
import { LeadDetailWidgets } from "../components/LeadDetailWidgets";
import {
  formatLeadCurrency,
  formatLeadDate,
  isOpenLead,
  leadDisplayName,
  leadTopicLine,
} from "../leadHubUtils";
import { d365Card } from "../../../theme/crmDynamicsTheme";
import { useTableSort, SortableTh } from "../../../../../components";

const cmdBtn =
  "inline-flex items-center gap-1 px-2.5 py-1 text-sm font-medium text-[#323130] bg-white border border-[#8A8886] hover:bg-[#F3F2F1] rounded-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed";
const cmdBtnPrimary =
  "inline-flex items-center gap-1 px-2.5 py-1 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] rounded-sm transition-colors disabled:opacity-50";

const StatusPill: React.FC<{ status?: string }> = ({ status }) => {
  const s = (status || "New").toLowerCase();
  const cls =
    s === "qualified" || s === "converted"
      ? "bg-[#DFF6DD] text-[#107C10]"
      : s === "lost"
        ? "bg-[#FDE7E9] text-[#A4262C]"
        : "bg-[#EFF6FC] text-[#0078D4]";
  return (
    <span className={`inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium ${cls}`}>{status || "New"}</span>
  );
};

interface LeadHubProps {
  onQuickCreate?: () => void;
}

const LeadHub: React.FC<LeadHubProps> = ({ onQuickCreate }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const selectedId = searchParams.get("id");

  const [leads, setLeads] = useState<LeadDto[]>([]);
  const [activities, setActivities] = useState<ActivityDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [activitiesLoading, setActivitiesLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [viewFilter, setViewFilter] = useState<"open" | "all">("open");
  const [actionLoading, setActionLoading] = useState(false);
  const [savedFlash, setSavedFlash] = useState(false);
  const summaryFormRef = useRef<LeadSummaryFieldsHandle>(null);
  const loadLeads = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await leadService.getAll();
      setLeads(data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; error?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.error || "Failed to load leads");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadLeads();
  }, [loadLeads]);

  const { sortKey: activitySortKey, sortDir: activitySortDir, toggleSort: toggleActivitySort, sortRows: sortActivityRows } =
    useTableSort("startDate", "desc");
  const sortedActivities = useMemo(
    () => sortActivityRows(activities),
    [activities, sortActivityRows]
  );

  const filteredLeads = useMemo(() => {
    let list = viewFilter === "open" ? leads.filter(isOpenLead) : leads;
    const q = search.trim().toLowerCase();
    if (q) {
      list = list.filter((l) => {
        const hay = [
          l.firstName,
          l.lastName,
          l.companyName,
          l.email,
          l.leadCode,
          l.notes,
        ]
          .filter(Boolean)
          .join(" ")
          .toLowerCase();
        return hay.includes(q);
      });
    }
    return [...list].sort((a, b) => {
      const da = new Date(b.updatedAt || b.createdAt || 0).getTime();
      const db = new Date(a.updatedAt || a.createdAt || 0).getTime();
      return da - db;
    });
  }, [leads, search, viewFilter]);

  const selectedLead = useMemo(() => {
    if (selectedId) {
      return filteredLeads.find((l) => l.id === selectedId) ?? leads.find((l) => l.id === selectedId);
    }
    return filteredLeads[0];
  }, [filteredLeads, leads, selectedId]);

  useEffect(() => {
    if (!loading && filteredLeads.length > 0 && !selectedId && filteredLeads[0].id) {
      setSearchParams({ id: filteredLeads[0].id }, { replace: true });
    }
  }, [loading, filteredLeads, selectedId, setSearchParams]);

  const loadActivities = useCallback(async (leadId: string) => {
    setActivitiesLoading(true);
    try {
      const data = await activityService.getByLead(leadId);
      setActivities(data);
    } catch {
      setActivities([]);
    } finally {
      setActivitiesLoading(false);
    }
  }, []);

  useEffect(() => {
    if (selectedLead?.id) {
      void loadActivities(selectedLead.id);
    } else {
      setActivities([]);
    }
  }, [selectedLead?.id, loadActivities]);

  const selectLead = (id: string) => setSearchParams({ id });

  const refreshLead = async (id: string) => {
    const updated = await leadService.getById(id);
    setLeads((prev) => prev.map((l) => (l.id === id ? updated : l)));
    void loadActivities(id);
  };

  const updateStatus = async (status: string) => {
    if (!selectedLead?.id) return;
    setActionLoading(true);
    try {
      await leadService.update(selectedLead.id, {
        ...selectedLead,
        id: selectedLead.id,
        status,
      });
      await refreshLead(selectedLead.id);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to update lead");
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedLead?.id) return;
    if (!window.confirm("Delete this lead?")) return;
    setActionLoading(true);
    try {
      await leadService.delete(selectedLead.id);
      const remaining = leads.filter((l) => l.id !== selectedLead.id);
      setLeads(remaining);
      const next = remaining.find(isOpenLead) ?? remaining[0];
      if (next?.id) setSearchParams({ id: next.id });
      else setSearchParams({});
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to delete lead");
    } finally {
      setActionLoading(false);
    }
  };

  const handleConvertToCustomer = async () => {
    if (!selectedLead?.id) return;
    if (!window.confirm("Convert this lead to a customer?")) return;
    setActionLoading(true);
    try {
      await leadService.convertToCustomer(selectedLead.id);
      await loadLeads();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to convert lead");
    } finally {
      setActionLoading(false);
    }
  };

  const handleConvertToOpportunity = async () => {
    if (!selectedLead?.id) return;
    if (!window.confirm("Convert this lead to an opportunity?")) return;
    setActionLoading(true);
    try {
      const opportunityId = await leadService.convertToOpportunity(selectedLead.id);
      await loadLeads();
      if (opportunityId) {
        window.location.href = `/crm/opportunities/${opportunityId}/edit`;
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to convert lead to opportunity");
    } finally {
      setActionLoading(false);
    }
  };

  const handleCompleteActivity = async (activityId: string) => {
    setActionLoading(true);
    try {
      await activityService.complete(activityId);
      if (selectedLead?.id) {
        const acts = await activityService.getByLead(selectedLead.id);
        setActivities(acts);
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to complete activity");
    } finally {
      setActionLoading(false);
    }
  };

  const handleSaveLead = async () => {
    if (!selectedLead?.id) return;
    setActionLoading(true);
    try {
      const ok = await summaryFormRef.current?.save();
      if (ok) {
        await refreshLead(selectedLead.id);
        setSavedFlash(true);
        window.setTimeout(() => setSavedFlash(false), 2500);
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string } } };
      alert(e.response?.data?.Error || "Failed to save lead");
    } finally {
      setActionLoading(false);
    }
  };

  const saveLeadFields = async (data: Parameters<typeof leadService.update>[1]) => {
    if (!selectedLead?.id) return;
    await leadService.update(selectedLead.id, data);
  };

  return (
    <>

      {error && (
        <div className="mx-3 mt-3 px-4 py-2 rounded-sm border border-[#F1BBBC] bg-[#FDE7E9] text-[#A4262C] text-sm">
          {error}
        </div>
      )}

      <div className="flex gap-0 min-h-[640px] overflow-hidden">
        {/* Left â€” lead list */}
        <aside className="w-full max-w-[340px] shrink-0 border-r border-[#EDEBE9] dark:border-[#3b3a39] flex flex-col bg-[#FAF9F8] dark:bg-[#292827]">
          <div className="px-3 py-2.5 border-b border-[#EDEBE9] dark:border-[#3b3a39]">
            <div className="flex items-center justify-between gap-2 mb-2">
              <select
                value={viewFilter}
                onChange={(e) => setViewFilter(e.target.value as "open" | "all")}
                className="text-sm font-semibold text-[#323130] bg-transparent border-0 p-0 cursor-pointer focus:ring-0"
              >
                <option value="open">My Open Leads</option>
                <option value="all">All Leads</option>
              </select>
              <button type="button" onClick={() => void loadLeads()} className="p-1 text-[#605E5C] hover:text-[#0078D4]" title="Refresh">
                <i className="mgc_refresh_1_line text-lg" />
              </button>
            </div>
            <div className="relative">
              <i className="mgc_search_line absolute left-2.5 top-1/2 -translate-y-1/2 text-[#8A8886]" />
              <input
                type="search"
                placeholder="Search work items"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-8 pr-3 py-1.5 text-sm border border-[#8A8886] rounded-sm bg-white focus:outline-none focus:border-[#0078D4] focus:ring-1 focus:ring-[#0078D4]"
              />
            </div>
          </div>

          <SimpleBar className="flex-1" style={{ maxHeight: "calc(100vh - 220px)" }}>
            {loading ? (
              <p className="text-center text-sm text-[#605E5C] py-8">Loadingâ€¦</p>
            ) : filteredLeads.length === 0 ? (
              <div className="p-4 text-center">
                <p className="text-sm text-[#605E5C] mb-3">No leads found</p>
                <button
                  type="button"
                  onClick={onQuickCreate}
                  className="text-sm text-[#0078D4] hover:underline font-medium"
                >
                  Create a lead
                </button>
              </div>
            ) : (
              <ul className="divide-y divide-[#EDEBE9] dark:divide-[#3b3a39]">
                {filteredLeads.map((lead) => {
                  const active = lead.id === selectedLead?.id;
                  return (
                    <li key={lead.id}>
                      <button
                        type="button"
                        onClick={() => lead.id && selectLead(lead.id)}
                        className={`w-full text-left px-3 py-3 transition-colors border-l-[3px] ${
                          active
                            ? "bg-white dark:bg-[#1f1f1f] border-l-[#0078D4]"
                            : "bg-transparent border-l-transparent hover:bg-[#F3F2F1] dark:hover:bg-[#323130]"
                        }`}
                      >
                        <p className="text-sm font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0.5 truncate">
                          {leadDisplayName(lead)}
                        </p>
                        <p className="text-xs text-[#605E5C] mb-1.5 line-clamp-2">{leadTopicLine(lead)}</p>
                        <div className="flex items-center gap-2 flex-wrap">
                          <StatusPill status={lead.status} />
                          <span className="text-[11px] text-[#8A8886]">
                            {formatLeadDate(lead.createdAt)}
                          </span>
                        </div>
                      </button>
                    </li>
                  );
                })}
              </ul>
            )}
          </SimpleBar>

          <div className="px-3 py-2 border-t border-[#EDEBE9] text-[11px] text-[#605E5C]">
            {filteredLeads.length > 0
              ? `1 â€“ ${filteredLeads.length} of ${filteredLeads.length}`
              : "0 records"}
          </div>
        </aside>

        {/* Right â€” record */}
        <main className="flex-1 min-w-0 flex flex-col bg-white dark:bg-[#1f1f1f]">
          {!selectedLead ? (
            <div className="flex flex-1 items-center justify-center text-[#605E5C] text-sm">
              {loading ? "Loadingâ€¦" : "Select a lead from the list"}
            </div>
          ) : (
            <>
              {/* Command bar */}
              <div className="flex flex-wrap items-center gap-1 px-3 py-2 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-[#FAF9F8] dark:bg-[#292827]">
                <button
                  type="button"
                  className={cmdBtnPrimary}
                  disabled={actionLoading}
                  onClick={() => void handleSaveLead()}
                >
                  <i className="mgc_save_line" /> Save
                </button>
                <button type="button" className={cmdBtn} disabled={actionLoading} onClick={() => void handleDelete()}>
                  Delete
                </button>
                {selectedLead.status !== "Qualified" && selectedLead.status !== "Converted" && (
                  <button
                    type="button"
                    className={cmdBtnPrimary}
                    disabled={actionLoading}
                    onClick={() => void updateStatus("Qualified")}
                  >
                    Qualify
                  </button>
                )}
                {selectedLead.status !== "Lost" && selectedLead.status !== "Converted" && (
                  <>
                    <button
                      type="button"
                      className={cmdBtn}
                      disabled={actionLoading}
                      onClick={() => void updateStatus("Lost")}
                    >
                      Disqualify
                    </button>
                    <button
                      type="button"
                      className={cmdBtn}
                      disabled={actionLoading}
                      onClick={() => void handleConvertToOpportunity()}
                    >
                      To opportunity
                    </button>
                    <button
                      type="button"
                      className={cmdBtn}
                      disabled={actionLoading}
                      onClick={() => void handleConvertToCustomer()}
                    >
                      To customer
                    </button>
                  </>
                )}
                <Link
                  to={`/crm/activities/new?leadId=${selectedLead.id}`}
                  className={`${cmdBtn} ms-auto`}
                >
                  <i className="mgc_calendar_line" /> Activity
                </Link>
              </div>

              <LeadDetailHeader lead={selectedLead} saved={savedFlash} />
              <LeadBusinessProcessFlow lead={selectedLead} />

              <Tab.Group as="div" className="flex flex-col flex-1 min-h-0">
                <div className="flex items-center border-b border-[#EDEBE9] dark:border-[#3b3a39]">
                  <Tab.List className="flex flex-1 gap-0 px-2">
                    {["Summary", "Relationship Analytics", "Details", "Related"].map((tab) => (
                      <Tab
                        key={tab}
                        className={({ selected }) =>
                          `px-4 py-2.5 text-sm font-medium outline-none border-b-2 -mb-px ${
                            selected
                              ? "text-[#0078D4] border-[#0078D4]"
                              : "text-[#605E5C] border-transparent hover:text-[#323130]"
                          }`
                        }
                      >
                        {tab}
                      </Tab>
                    ))}
                  </Tab.List>
                  <button
                    type="button"
                    className="shrink-0 me-3 px-3 py-1.5 text-sm text-[#0078D4] border border-[#0078D4] rounded-sm hover:bg-[#EFF6FC]"
                  >
                    Form assist
                  </button>
                </div>

                <Tab.Panels className="flex-1 overflow-auto">
                  <Tab.Panel className="p-4">
                    <div className="grid grid-cols-1 xl:grid-cols-12 gap-4">
                      <div className="xl:col-span-4 min-w-0">
                        <LeadSummaryFields
                          ref={summaryFormRef}
                          lead={selectedLead}
                          onSave={saveLeadFields}
                        />
                      </div>
                      <div className="xl:col-span-4 min-w-0">
                        <LeadUpNextPanel
                          leadId={selectedLead.id!}
                          leadEmail={selectedLead.email}
                          activities={activities}
                          loading={activitiesLoading}
                          onCompleteActivity={(id) => void handleCompleteActivity(id)}
                          actionLoading={actionLoading}
                        />
                      </div>
                      <div className="xl:col-span-4 min-w-0">
                        <LeadDetailWidgets lead={selectedLead} />
                      </div>
                    </div>
                  </Tab.Panel>

                  <Tab.Panel className="p-4">
                    <div className={`${d365Card} p-8 text-center`}>
                      <p className="text-sm text-[#605E5C] mb-0">Relationship analytics coming soon.</p>
                    </div>
                  </Tab.Panel>

                  <Tab.Panel className="p-4">
                    <div className={`${d365Card} p-4`}>
                      <dl className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4 text-sm">
                        {[
                          ["Lead code", selectedLead.leadCode],
                          ["Company", selectedLead.companyName || "â€”"],
                          ["Email", selectedLead.email || "â€”"],
                          ["Phone", selectedLead.phone || "â€”"],
                          ["Industry", selectedLead.industry || "â€”"],
                          ["Estimated value", selectedLead.estimatedValue != null ? formatLeadCurrency(selectedLead.estimatedValue) : "â€”"],
                          ["Last contact", formatLeadDate(selectedLead.lastContactDate)],
                          ["Priority", selectedLead.priority || "â€”"],
                        ].map(([label, value]) => (
                          <div key={String(label)}>
                            <dt className="text-[#8A8886] text-xs mb-0.5">{label}</dt>
                            <dd className="text-[#323130] font-medium mb-0">{value}</dd>
                          </div>
                        ))}
                      </dl>
                      {selectedLead.notes && (
                        <div className="mt-4 pt-4 border-t border-[#EDEBE9]">
                          <dt className="text-[#8A8886] text-xs mb-1">Notes</dt>
                          <dd className="text-sm text-[#323130] mb-0 whitespace-pre-wrap">{selectedLead.notes}</dd>
                        </div>
                      )}
                      <div className="mt-4">
                        <Link
                          to={`/crm/leads/${selectedLead.id}/edit`}
                          className="text-sm text-[#0078D4] hover:underline font-medium"
                        >
                          Edit all fields â†’
                        </Link>
                      </div>
                    </div>
                  </Tab.Panel>

                  <Tab.Panel className="p-4">
                    <div className={d365Card}>
                      <div className="px-4 py-3 border-b border-[#EDEBE9] flex justify-between items-center">
                        <h3 className="text-[15px] font-semibold text-[#323130] mb-0">Activities</h3>
                        <Link
                          to={`/crm/activities/new?leadId=${selectedLead.id}`}
                          className="text-sm text-[#0078D4] hover:underline font-medium"
                        >
                          New activity
                        </Link>
                      </div>
                      <div className="table-responsive">
                        <table className="w-full text-sm mb-0 [&_th]:text-left [&_th]:text-[11px] [&_th]:font-semibold [&_th]:text-[#605E5C] [&_th]:px-4 [&_th]:py-2 [&_th]:bg-[#FAF9F8] [&_td]:px-4 [&_td]:py-2 [&_td]:border-t [&_td]:border-[#EDEBE9]">
                          <thead>
                            <tr>
                              <SortableTh variant="crm" columnKey="subject" label="Subject" sortKey={activitySortKey} sortDir={activitySortDir} onSort={toggleActivitySort} />
                              <SortableTh variant="crm" columnKey="activityType" label="Type" sortKey={activitySortKey} sortDir={activitySortDir} onSort={toggleActivitySort} />
                              <SortableTh variant="crm" columnKey="status" label="Status" sortKey={activitySortKey} sortDir={activitySortDir} onSort={toggleActivitySort} />
                              <SortableTh variant="crm" columnKey="startDate" label="Date" sortKey={activitySortKey} sortDir={activitySortDir} onSort={toggleActivitySort} />
                            </tr>
                          </thead>
                          <tbody>
                            {activities.length === 0 ? (
                              <tr>
                                <td colSpan={4} className="text-center text-[#605E5C] py-8">
                                  No related activities
                                </td>
                              </tr>
                            ) : (
                              sortedActivities.map((a) => (
                                <tr key={a.id}>
                                  <td>
                                    <Link to={`/crm/activities/${a.id}/edit`} className="text-[#0078D4] hover:underline">
                                      {a.subject}
                                    </Link>
                                  </td>
                                  <td>{a.activityType}</td>
                                  <td>{a.status}</td>
                                  <td>{formatLeadDate(a.startDate)}</td>
                                </tr>
                              ))
                            )}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </Tab.Panel>
                </Tab.Panels>
              </Tab.Group>
            </>
          )}
        </main>
      </div>
    </>
  );
};

export default LeadHub;

