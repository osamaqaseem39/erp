import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useCallback, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
  DataTable,
  DataTableToolbar,
  EmptyState,
  FilterChips,
  PageHeader,
  Pagination,
  SearchInput,
  type Column,
} from "../../../components/erp";
import { formatDateTime } from "../../../lib/erp/format";
import { usePagedQuery } from "../../../lib/erp/http";
import { queryFromSearchParams, searchParamsFromQuery, type PagedQuery } from "../../../lib/erp/paging";
import { coreApi, type AuditLogEntry, type AuditLogFilters } from "../services/coreApi";
import { diffAuditEntry, REDACTED } from "../utils/coreFormat";

const FILTER_KEYS = ["entityName", "action", "from", "to"] as const;
const ACTIONS = ["Created", "Updated", "Deleted"];
const humanize = (field: string) => field.replace(/([a-z])([A-Z])/g, "$1 $2").replace(/^./, (c) => c.toUpperCase());

function Value({ value }: { value: unknown }) {
  if (value === undefined || value === null || value === "") return <span className="text-slate-400">—</span>;
  if (value === REDACTED) return <span className="italic text-slate-500">hidden</span>;
  if (typeof value === "string" && /^\d{4}-\d{2}-\d{2}T/.test(value)) return <>{formatDateTime(value)}</>;
  return <span className="break-words">{typeof value === "object" ? JSON.stringify(value) : String(value)}</span>;
}

const columns: Column<AuditLogEntry>[] = [
  { key: "timestamp", header: "When", sortable: true, hideable: false, cell: (a) => formatDateTime(a.timestamp) },
  { key: "entityName", header: "Record", sortable: true, cell: (a) => humanize(a.entityName) },
  { key: "action", header: "Change", sortable: true, cell: (a) => a.action },
  { key: "fields", header: "Fields", cell: (a) => <span className="text-slate-500">{diffAuditEntry(a).map((c) => humanize(c.field)).slice(0, 4).join(", ") || "—"}</span> },
  { key: "user", header: "By", cell: (a) => a.userName ?? (a.userId ? "User" : "System") },
];

export default function AuditLogExplorer() {
  const [params, setParams] = useSearchParams();
  const query = useMemo(() => queryFromSearchParams<AuditLogFilters>(params, { sortBy: "timestamp", sortDirection: "desc", filterKeys: FILTER_KEYS }), [params]);
  const { data, error, isLoading, isRefreshing, refetch } = usePagedQuery(coreApi.auditLogs, query);
  const [selected, setSelected] = useState<AuditLogEntry | null>(null);

  const setQuery = useCallback(
    (next: Partial<PagedQuery<AuditLogFilters>>, resetPage = true) =>
      setParams(searchParamsFromQuery({ ...query, ...next, pageNumber: resetPage ? 1 : next.pageNumber ?? query.pageNumber }), { replace: true }),
    [query, setParams]
  );
  const filters = query.filters ?? {};
  const setFilter = (key: keyof AuditLogFilters, value: string) => setQuery({ filters: { ...filters, [key]: value || undefined } });
  const isFiltered = !!query.searchTerm || Object.values(filters).some(Boolean);
  const chips = FILTER_KEYS.filter((k) => filters[k]).map((k) => ({ key: k, label: `${humanize(k)}: ${filters[k]}` }));

  return (
    <>
      <PageHeader
        title="Audit log"
        description="Every change to your organization's records: who made it, when, and what changed. Entries can't be edited or deleted."
        breadcrumbs={[{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: "Audit log" }]}
      />
      <div className="card">
        <DataTableToolbar
          search={<SearchInput value={query.searchTerm ?? ""} onSearch={(v) => setQuery({ searchTerm: v || undefined })} label="Search the audit log" placeholder="Search record type, id or change" />}
          filters={
            <>
              <label className="sr-only" htmlFor="audit-action">
                Change
              </label>
              <select id="audit-action" className="form-select w-auto" value={filters.action ?? ""} onChange={(e) => setFilter("action", e.target.value)}>
                <option value="">All changes</option>
                {ACTIONS.map((a) => (
                  <option key={a}>{a}</option>
                ))}
              </select>
              <label className="sr-only" htmlFor="audit-from">
                From date
              </label>
              <input id="audit-from" type="date" className="form-input w-auto" value={filters.from ?? ""} onChange={(e) => setFilter("from", e.target.value)} />
              <label className="sr-only" htmlFor="audit-to">
                To date
              </label>
              <input id="audit-to" type="date" className="form-input w-auto" value={filters.to ?? ""} onChange={(e) => setFilter("to", e.target.value)} />
            </>
          }
          onRefresh={refetch}
          isRefreshing={isRefreshing}
        />
        <FilterChips chips={chips} onRemove={(k) => setFilter(k as keyof AuditLogFilters, "")} onClearAll={() => setQuery({ filters: {}, searchTerm: undefined })} />
        <DataTable<AuditLogEntry>
          caption="Audit log"
          columns={columns}
          rows={data?.items}
          getRowId={(a) => a.id}
          isLoading={isLoading}
          isRefreshing={isRefreshing}
          error={error}
          onRetry={refetch}
          sort={{ key: query.sortBy ?? "timestamp", direction: query.sortDirection ?? "desc" }}
          onSortChange={(s) => setQuery({ sortBy: s.key, sortDirection: s.direction }, false)}
          rowActions={[{ label: "View changes", icon: "mgc_eye_line", onSelect: setSelected }]}
          isFiltered={isFiltered}
          emptyState={<EmptyState title="No changes recorded yet" description="Creating, editing or deleting records adds entries here automatically." />}
          noResultsState={<EmptyState variant="no-results" />}
        />
        {data && data.totalCount > 0 && (
          <Pagination
            pageNumber={data.pageNumber}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
            onPageChange={(pageNumber) => setQuery({ pageNumber }, false)}
            onPageSizeChange={(pageSize) => setQuery({ pageSize })}
            noun={["entry", "entries"]}
          />
        )}
      </div>
      <AuditEntryDrawer entry={selected} onClose={() => setSelected(null)} />
    </>
  );
}

export function AuditEntryDrawer({ entry, onClose }: { entry: AuditLogEntry | null; onClose: () => void }) {
  const changes = entry ? diffAuditEntry(entry) : [];
  return (
    <Dialog open={!!entry} onClose={onClose} className="relative z-[60]">
      <div className="fixed inset-0 bg-slate-900/40" aria-hidden="true" />
      <div className="fixed inset-y-0 end-0 flex w-full max-w-lg">
        <DialogPanel className="flex w-full flex-col overflow-y-auto bg-white shadow-xl dark:bg-gray-800">
          {entry && (
            <>
              <div className="flex items-start justify-between border-b border-slate-200 px-5 py-4 dark:border-slate-700">
                <div>
                  <DialogTitle className="text-base font-semibold">
                    {humanize(entry.entityName)} {entry.action.toLowerCase()}
                  </DialogTitle>
                  <p className="text-sm text-slate-500">
                    {formatDateTime(entry.timestamp)}
                    {entry.userName ? ` by ${entry.userName}` : ""}
                  </p>
                </div>
                <button type="button" onClick={onClose} className="rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-700" aria-label="Close">
                  <i className="mgc_close_line text-lg" aria-hidden="true" />
                </button>
              </div>
              <div className="px-5 py-4">
                {changes.length === 0 ? (
                  <EmptyState compact title="No field details" description="This entry doesn't include field values (for example, the change was too large to store)." />
                ) : (
                  <table className="min-w-full text-sm">
                    <caption className="sr-only">Field changes</caption>
                    <thead>
                      <tr className="text-slate-500">
                        <th scope="col" className="pb-2 text-left font-medium normal-case tracking-normal">Field</th>
                        {entry.action !== "Created" && <th scope="col" className="pb-2 text-left font-medium normal-case tracking-normal">Before</th>}
                        {entry.action !== "Deleted" && <th scope="col" className="pb-2 text-left font-medium normal-case tracking-normal">After</th>}
                      </tr>
                    </thead>
                    <tbody>
                      {changes.map((c) => (
                        <tr key={c.field} className="border-t border-slate-100 align-top dark:border-slate-700">
                          <th scope="row" className="py-2 pe-3 text-left font-medium normal-case tracking-normal">{humanize(c.field)}</th>
                          {entry.action !== "Created" && (
                            <td className="py-2 pe-3 text-slate-500">
                              <Value value={c.before} />
                            </td>
                          )}
                          {entry.action !== "Deleted" && (
                            <td className="py-2">
                              <Value value={c.after} />
                            </td>
                          )}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
                <dl className="mt-5 grid grid-cols-2 gap-3 border-t border-slate-100 pt-4 text-xs text-slate-500 dark:border-slate-700">
                  <div>
                    <dt>Record id</dt>
                    <dd className="break-all">{entry.entityId}</dd>
                  </div>
                  <div>
                    <dt>IP address</dt>
                    <dd>{entry.ipAddress || "—"}</dd>
                  </div>
                </dl>
              </div>
            </>
          )}
        </DialogPanel>
      </div>
    </Dialog>
  );
}
