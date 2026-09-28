import { useCallback, useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import {
  ColumnSelector,
  DataTable,
  DataTableToolbar,
  DeleteDialog,
  EmptyState,
  FilterChips,
  FilterPanel,
  PageHeader,
  Pagination,
  PermissionGuard,
  SearchInput,
  StatusBadge,
  toast,
  type Column,
  type RowAction,
} from "../../../../../components/erp";
import { formatDate, formatMoney, orDash } from "../../../../../lib/erp/format";
import { usePagedQuery } from "../../../../../lib/erp/http";
import { queryFromSearchParams, searchParamsFromQuery, type PagedQuery } from "../../../../../lib/erp/paging";
import { usePermission } from "../../../../../lib/erp/permissions";
import { useColumnVisibility } from "../../../../../lib/erp/useColumnVisibility";
import { customerService, type CustomerFilters } from "../services";
import { CUSTOMER_STATUS_OPTIONS, CUSTOMER_TYPE_OPTIONS, type CustomerDto } from "../types";
import { customerDisplayName, markCustomerInactive } from "../customerUtils";

const FILTER_KEYS = ["status", "industry", "customerType"] as const;
const DEFAULTS = { sortBy: "customerName", sortDirection: "asc" as const, filterKeys: FILTER_KEYS };

const columns: Column<CustomerDto>[] = [
  {
    key: "customerName",
    header: "Customer",
    sortable: true,
    hideable: false,
    cell: (c) => (
      <span className="flex flex-col">
        <span>{c.customerName}</span>
        {c.companyName && c.companyName !== c.customerName && <span className="text-xs font-normal text-slate-500">{c.companyName}</span>}
      </span>
    ),
  },
  { key: "customerCode", header: "Code", sortable: true, cell: (c) => <span className="text-slate-500">{c.customerCode}</span> },
  { key: "customerType", header: "Type", sortable: true, cell: (c) => c.customerType },
  { key: "status", header: "Status", sortable: true, cell: (c) => <StatusBadge status={c.status} /> },
  { key: "industry", header: "Industry", sortable: true, cell: (c) => orDash(c.industry), defaultHidden: true },
  { key: "email", header: "Email", sortable: true, cell: (c) => orDash(c.email) },
  { key: "phone", header: "Phone", cell: (c) => orDash(c.phone), defaultHidden: true },
  { key: "creditLimit", header: "Credit limit", sortable: true, align: "right", cell: (c) => <span className="tabular-nums">{formatMoney(c.creditLimit, c.currencyCode)}</span> },
  { key: "createdAt", header: "Created", sortable: true, cell: (c) => formatDate(c.createdAt), defaultHidden: true },
];

const detailUrl = (c: CustomerDto) => `/crm/customers/${c.id}`;

export default function CustomerList() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const query = useMemo(() => queryFromSearchParams<CustomerFilters>(params, DEFAULTS), [params]);
  const { data, error, isLoading, isRefreshing, refetch } = usePagedQuery(customerService.list, query);
  const columnState = useColumnVisibility("crm.customers", columns);
  const canCreate = usePermission("crm.customers.create");
  const canEdit = usePermission("crm.customers.update");
  const canDelete = usePermission("crm.customers.delete");

  const [filtersOpen, setFiltersOpen] = useState(false);
  const [draft, setDraft] = useState<Partial<CustomerFilters>>({});
  const [toDelete, setToDelete] = useState<CustomerDto | null>(null);

  const setQuery = useCallback(
    (next: Partial<PagedQuery<CustomerFilters>>, resetPage = true) => {
      const merged = { ...query, ...next, pageNumber: resetPage ? 1 : next.pageNumber ?? query.pageNumber };
      setParams(searchParamsFromQuery(merged), { replace: true });
    },
    [query, setParams]
  );

  const filters = query.filters ?? {};
  const setFilter = (key: keyof CustomerFilters, value: string | undefined) => setQuery({ filters: { ...filters, [key]: value || undefined } });
  const clearFilters = () => setQuery({ filters: {}, searchTerm: undefined });
  const isFiltered = !!query.searchTerm || Object.values(filters).some(Boolean);

  const chips = [
    filters.status && { key: "status", label: `Status: ${filters.status}` },
    filters.customerType && { key: "customerType", label: `Type: ${filters.customerType}` },
    filters.industry && { key: "industry", label: `Industry: ${filters.industry}` },
  ].filter(Boolean) as { key: keyof CustomerFilters; label: string }[];

  const rowActions: RowAction<CustomerDto>[] = [
    { label: "View", icon: "mgc_eye_line", onSelect: (c) => navigate(detailUrl(c)) },
    ...(canEdit ? [{ label: "Edit", icon: "mgc_edit_line", onSelect: (c: CustomerDto) => navigate(`${detailUrl(c)}/edit`) }] : []),
    ...(canDelete ? [{ label: "Delete", icon: "mgc_delete_2_line", tone: "danger" as const, onSelect: (c: CustomerDto) => setToDelete(c) }] : []),
  ];

  const addButton = canCreate && (
    <Link to="/crm/customers/new" className="btn bg-primary text-white">
      <i className="mgc_add_line me-1" aria-hidden="true" />
      Add customer
    </Link>
  );

  return (
    <>
      <PageHeader
        title="Customers"
        description="People and companies you sell to."
        breadcrumbs={[{ label: "CRM", to: "/crm/dashboard" }, { label: "Customers" }]}
        actions={addButton}
      />

      <div className="card">
        <DataTableToolbar
          search={
            <SearchInput
              value={query.searchTerm ?? ""}
              onSearch={(searchTerm) => setQuery({ searchTerm: searchTerm || undefined })}
              placeholder="Search by name, code, company, email or phone"
              label="Search customers"
            />
          }
          filters={
            <>
              <label className="sr-only" htmlFor="customer-status-filter">
                Status
              </label>
              <select
                id="customer-status-filter"
                className="form-select w-auto"
                value={filters.status ?? ""}
                onChange={(e) => setFilter("status", e.target.value)}
              >
                <option value="">All statuses</option>
                {CUSTOMER_STATUS_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
              <button
                type="button"
                className="btn border border-slate-300 dark:border-slate-600"
                onClick={() => {
                  setDraft(filters);
                  setFiltersOpen(true);
                }}
              >
                <i className="mgc_filter_line me-1" aria-hidden="true" />
                More filters
              </button>
            </>
          }
          tools={<ColumnSelector columns={columns} hidden={columnState.hidden} onToggle={columnState.toggle} onReset={columnState.reset} />}
          onRefresh={refetch}
          isRefreshing={isRefreshing}
        />
        <FilterChips chips={chips} onRemove={(key) => setFilter(key as keyof CustomerFilters, undefined)} onClearAll={clearFilters} />

        <DataTable<CustomerDto>
          caption="Customers"
          columns={columns}
          rows={data?.items}
          getRowId={(c) => c.id ?? c.customerCode}
          hiddenColumns={columnState.hidden}
          isLoading={isLoading}
          isRefreshing={isRefreshing}
          error={error}
          onRetry={refetch}
          sort={query.sortBy ? { key: query.sortBy, direction: query.sortDirection ?? "asc" } : undefined}
          onSortChange={(sort) => setQuery({ sortBy: sort.key, sortDirection: sort.direction }, false)}
          rowHref={detailUrl}
          rowActions={rowActions}
          isFiltered={isFiltered}
          emptyState={<EmptyState title="No customers yet" description="Add your first customer to start creating quotes and orders." action={addButton || undefined} />}
          noResultsState={
            <EmptyState
              variant="no-results"
              action={
                <button type="button" className="btn border border-slate-300 dark:border-slate-600" onClick={clearFilters}>
                  Clear search and filters
                </button>
              }
            />
          }
        />

        {data && data.totalCount > 0 && (
          <Pagination
            pageNumber={data.pageNumber}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
            onPageChange={(pageNumber) => setQuery({ pageNumber }, false)}
            onPageSizeChange={(pageSize) => setQuery({ pageSize })}
            noun={["customer", "customers"]}
          />
        )}
      </div>

      <FilterPanel
        open={filtersOpen}
        onClose={() => setFiltersOpen(false)}
        onClear={() => setDraft({})}
        onApply={() => {
          setQuery({ filters: draft });
          setFiltersOpen(false);
        }}
        title="Filter customers"
      >
        <label className="block text-sm font-medium">
          Status
          <select className="form-select mt-1 w-full" value={draft.status ?? ""} onChange={(e) => setDraft({ ...draft, status: e.target.value || undefined })}>
            <option value="">Any</option>
            {CUSTOMER_STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm font-medium">
          Customer type
          <select className="form-select mt-1 w-full" value={draft.customerType ?? ""} onChange={(e) => setDraft({ ...draft, customerType: e.target.value || undefined })}>
            <option value="">Any</option>
            {CUSTOMER_TYPE_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm font-medium">
          Industry
          <input className="form-input mt-1 w-full" value={draft.industry ?? ""} onChange={(e) => setDraft({ ...draft, industry: e.target.value || undefined })} placeholder="Exact industry name" />
        </label>
      </FilterPanel>

      <PermissionGuard permission="crm.customers.delete">
        <DeleteDialog
          open={!!toDelete}
          entityName="customer"
          recordLabel={toDelete ? customerDisplayName(toDelete) : ""}
          onDelete={() => customerService.remove(toDelete!.id!)}
          onArchive={toDelete?.status !== "Inactive" && canEdit ? () => markCustomerInactive(toDelete!) : undefined}
          onDone={(action) => {
            toast.success(action === "deleted" ? "Customer deleted" : "Customer marked inactive");
            setToDelete(null);
            refetch();
          }}
          onCancel={() => setToDelete(null)}
        />
      </PermissionGuard>
    </>
  );
}
