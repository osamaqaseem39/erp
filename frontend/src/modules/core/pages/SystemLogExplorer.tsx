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
  StatusBadge,
  type Column,
} from "../../../components/erp";
import { formatDateTime } from "../../../lib/erp/format";
import { usePagedQuery } from "../../../lib/erp/http";
import { queryFromSearchParams, searchParamsFromQuery, type PagedQuery } from "../../../lib/erp/paging";
import { coreApi, type SystemLogEntry, type SystemLogFilters } from "../services/coreApi";

const FILTER_KEYS = ["logLevel", "source", "from", "to"] as const;
const levelBadge = (level: string) => <StatusBadge status={level} />;

const columns: Column<SystemLogEntry>[] = [
  { key: "timestamp", header: "When", sortable: true, hideable: false, cell: (l) => formatDateTime(l.createdAt) },
  { key: "logLevel", header: "Level", sortable: true, cell: (l) => levelBadge(l.logLevel) },
  { key: "source", header: "Source", sortable: true, cell: (l) => l.source ?? "—" },
  { key: "message", header: "Message", cell: (l) => <span className="line-clamp-2">{l.message}</span> },
];

export default function SystemLogExplorer() {
  const [params, setParams] = useSearchParams();
  const query = useMemo(() => queryFromSearchParams<SystemLogFilters>(params, { sortBy: "timestamp", sortDirection: "desc", filterKeys: FILTER_KEYS }), [params]);
  const { data, error, isLoading, isRefreshing, refetch } = usePagedQuery(coreApi.systemLogs, query);
  const [selected, setSelected] = useState<SystemLogEntry | null>(null);

  const setQuery = useCallback(
    (next: Partial<PagedQuery<SystemLogFilters>>, resetPage = true) =>
      setParams(searchParamsFromQuery({ ...query, ...next, pageNumber: resetPage ? 1 : next.pageNumber ?? query.pageNumber }), { replace: true }),
    [query, setParams]
  );
  const filters = query.filters ?? {};
  const setFilter = (key: keyof SystemLogFilters, value: string) => setQuery({ filters: { ...filters, [key]: value || undefined } });
  const isFiltered = !!query.searchTerm || Object.values(filters).some(Boolean);
  const chips = FILTER_KEYS.filter((k) => filters[k]).map((k) => ({ key: k, label: `${k === "logLevel" ? "Level" : k[0].toUpperCase() + k.slice(1)}: ${filters[k]}` }));

  return (
    <>
      <PageHeader
        title="System log"
        description="Server errors and security events such as failed sign-ins. Share the trace reference with support when reporting a problem."
        breadcrumbs={[{ label: "Core / Settings", to: "/core" }, { label: "Settings", to: "/core/settings" }, { label: "System log" }]}
      />
      <div className="card">
        <DataTableToolbar
          search={<SearchInput value={query.searchTerm ?? ""} onSearch={(v) => setQuery({ searchTerm: v || undefined })} label="Search messages" placeholder="Search messages or trace ids" />}
          filters={
            <>
              <label className="sr-only" htmlFor="log-level">
                Level
              </label>
              <select id="log-level" className="form-select w-auto" value={filters.logLevel ?? ""} onChange={(e) => setFilter("logLevel", e.target.value)}>
                <option value="">All levels</option>
                <option>Error</option>
                <option>Warning</option>
                <option>Information</option>
              </select>
              <label className="sr-only" htmlFor="log-source">
                Source
              </label>
              <select id="log-source" className="form-select w-auto" value={filters.source ?? ""} onChange={(e) => setFilter("source", e.target.value)}>
                <option value="">All sources</option>
                <option value="Api">Server</option>
                <option value="Security">Security</option>
              </select>
              <label className="sr-only" htmlFor="log-from">
                From date
              </label>
              <input id="log-from" type="date" className="form-input w-auto" value={filters.from ?? ""} onChange={(e) => setFilter("from", e.target.value)} />
            </>
          }
          onRefresh={refetch}
          isRefreshing={isRefreshing}
        />
        <FilterChips chips={chips} onRemove={(k) => setFilter(k as keyof SystemLogFilters, "")} onClearAll={() => setQuery({ filters: {}, searchTerm: undefined })} />
        <DataTable<SystemLogEntry>
          caption="System log"
          columns={columns}
          rows={data?.items}
          getRowId={(l) => l.id}
          isLoading={isLoading}
          isRefreshing={isRefreshing}
          error={error}
          onRetry={refetch}
          sort={{ key: query.sortBy ?? "timestamp", direction: query.sortDirection ?? "desc" }}
          onSortChange={(s) => setQuery({ sortBy: s.key, sortDirection: s.direction }, false)}
          rowActions={[{ label: "View details", icon: "mgc_eye_line", onSelect: setSelected }]}
          isFiltered={isFiltered}
          emptyState={<EmptyState title="Nothing logged yet" description="Server errors and security events for your organization appear here." />}
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

      <Dialog open={!!selected} onClose={() => setSelected(null)} className="relative z-[60]">
        <div className="fixed inset-0 bg-slate-900/40" aria-hidden="true" />
        <div className="fixed inset-y-0 end-0 flex w-full max-w-2xl">
          <DialogPanel className="flex w-full flex-col overflow-y-auto bg-white shadow-xl dark:bg-gray-800">
            {selected && (
              <>
                <div className="flex items-start justify-between border-b border-slate-200 px-5 py-4 dark:border-slate-700">
                  <div>
                    <DialogTitle className="text-base font-semibold">{selected.source ?? "System"} event</DialogTitle>
                    <p className="text-sm text-slate-500">{formatDateTime(selected.createdAt)}</p>
                  </div>
                  <button type="button" onClick={() => setSelected(null)} className="rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-700" aria-label="Close">
                    <i className="mgc_close_line text-lg" aria-hidden="true" />
                  </button>
                </div>
                <div className="space-y-4 px-5 py-4 text-sm">
                  <div>{levelBadge(selected.logLevel)}</div>
                  <p className="break-words">{selected.message}</p>
                  {selected.additionalData && (
                    <section>
                      <h3 className="mb-1 font-medium">Details</h3>
                      <pre className="overflow-x-auto rounded bg-slate-50 p-3 text-xs dark:bg-slate-900">{selected.additionalData}</pre>
                    </section>
                  )}
                  {selected.exception && (
                    <section>
                      <h3 className="mb-1 font-medium">Technical details</h3>
                      <pre className="max-h-96 overflow-auto rounded bg-slate-50 p-3 text-xs dark:bg-slate-900">{selected.exception}</pre>
                    </section>
                  )}
                </div>
              </>
            )}
          </DialogPanel>
        </div>
      </Dialog>
    </>
  );
}
