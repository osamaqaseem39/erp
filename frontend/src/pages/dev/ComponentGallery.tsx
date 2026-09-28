import { useState, type ReactNode } from "react";
import { FormProvider, useForm } from "react-hook-form";
import ReactApexChart from "react-apexcharts";
import {
  ActivityTimeline,
  BulkActionBar,
  ChartCard,
  CheckboxField,
  ColumnSelector,
  ConfirmDialog,
  CurrencyField,
  DataTable,
  DataTableToolbar,
  DateField,
  DeleteDialog,
  DetailHeader,
  DetailTabs,
  EmptyState,
  EntitySelect,
  ErrorState,
  FieldGrid,
  FilterChips,
  FilterPanel,
  FormErrorSummary,
  FormFooter,
  FormSection,
  FullWidth,
  KpiCard,
  LineItemsTable,
  NumberField,
  PageHeader,
  Pagination,
  PermissionDenied,
  RelatedRecordsPanel,
  SearchInput,
  SelectField,
  SkeletonCard,
  SkeletonFields,
  SkeletonTable,
  StatusBadge,
  TextareaField,
  TextField,
  WidgetBoundary,
  toast,
  type Column,
  type EntityOption,
} from "../../components/erp";
import { useColumnVisibility } from "../../lib/erp/useColumnVisibility";
import type { ApiErrorInfo } from "../../lib/erp/apiError";

/**
 * Development-only catalogue of the ERP UI kit (route /dev/components, not in production builds).
 * Everything below uses DEMO DATA defined in this file; nothing is fetched.
 */

const demoError: ApiErrorInfo = {
  status: 500,
  message: "Something went wrong. Please try again.",
  code: "INTERNAL_ERROR",
  fieldErrors: {},
  traceId: "00-demo-trace-01",
  isNetworkError: false,
  isCanceled: false,
};

interface DemoRow {
  id: string;
  code: string;
  name: string;
  status: string;
  balance: number;
}

const demoRows: DemoRow[] = [
  { id: "1", code: "C-001", name: "Demo customer A", status: "Active", balance: 1250 },
  { id: "2", code: "C-002", name: "Demo customer B", status: "Suspended", balance: 0 },
  { id: "3", code: "C-003", name: "Demo customer C", status: "Inactive", balance: 84210.5 },
];

const demoColumns: Column<DemoRow>[] = [
  { key: "name", header: "Name", cell: (r) => r.name, sortable: true, hideable: false },
  { key: "code", header: "Code", cell: (r) => r.code, sortable: true },
  { key: "status", header: "Status", cell: (r) => <StatusBadge status={r.status} /> },
  { key: "balance", header: "Balance", cell: (r) => `$${r.balance.toFixed(2)}`, align: "right", sortable: true },
];

const demoEntities: EntityOption[] = [
  { value: "e1", label: "Demo employee Ayesha", hint: "EMP-001" },
  { value: "e2", label: "Demo employee Bilal", hint: "EMP-002" },
];

function Section({ id, title, children }: { id: string; title: string; children: ReactNode }) {
  return (
    <section id={id} className="mb-10 scroll-mt-4">
      <h2 className="mb-3 border-b border-slate-200 pb-2 text-lg font-semibold dark:border-slate-700">{title}</h2>
      {children}
    </section>
  );
}

function Label({ children }: { children: ReactNode }) {
  return <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">{children}</p>;
}

const Thrower = () => {
  throw new Error("Demo render failure");
};

export default function ComponentGallery() {
  const [dark, setDark] = useState(document.documentElement.getAttribute("data-mode") === "dark");
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState<null | "ok" | "conflict">(null);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(3);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [sort, setSort] = useState<{ key: string; direction: "asc" | "desc" }>({ key: "name", direction: "asc" });
  const [breakWidget, setBreakWidget] = useState(false);
  const columnsState = useColumnVisibility("dev-gallery", demoColumns);

  const methods = useForm({
    defaultValues: {
      name: "",
      email: "not-an-email",
      type: "Company",
      creditLimit: 5000,
      quantity: undefined as number | undefined,
      startDate: "2026-09-29",
      managerId: "e2",
      notes: "",
      isVip: false,
      lines: [
        { description: "Demo item", quantity: 2, unitPrice: 49.99, discountPercent: 10, taxPercent: 17 },
        { description: "Demo service", quantity: 1, unitPrice: 120, discountPercent: 0, taxPercent: 0 },
      ],
    },
  });
  const [formMessages, setFormMessages] = useState<string[]>([]);

  const toggleDark = () => {
    const next = !dark;
    setDark(next);
    document.documentElement.setAttribute("data-mode", next ? "dark" : "light");
  };

  const sections = ["page", "status", "states", "kpi", "charts", "table", "dialogs", "forms", "detail"];

  return (
    <div className="min-h-screen bg-slate-50 p-6 text-slate-800 dark:bg-gray-900 dark:text-slate-100">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3 rounded-lg bg-warning/10 px-4 py-3 text-sm">
          <span>
            <strong>ERP UI kit gallery</strong> · development only · all figures are demo data defined in this page
          </span>
          <button type="button" className="btn btn-sm border border-slate-300 dark:border-slate-600" onClick={toggleDark} aria-pressed={dark}>
            {dark ? "Light mode" : "Dark mode"}
          </button>
        </div>
        <nav aria-label="Gallery sections" className="mb-8 flex flex-wrap gap-2 text-sm">
          {sections.map((s) => (
            <a key={s} href={`#${s}`} className="rounded bg-white px-2 py-1 capitalize shadow-sm hover:text-primary dark:bg-gray-800">
              {s}
            </a>
          ))}
        </nav>

        <Section id="page" title="PageHeader">
          <PageHeader
            title="Customers"
            description="People and companies you sell to."
            breadcrumbs={[{ label: "CRM", to: "/dev/components" }, { label: "Customers" }]}
            actions={
              <>
                <button type="button" className="btn border border-slate-300 dark:border-slate-600">Export</button>
                <button type="button" className="btn bg-primary text-white">Add customer</button>
              </>
            }
          />
        </Section>

        <Section id="status" title="StatusBadge (one tone per status, icon + label)">
          <div className="flex flex-wrap gap-2">
            {["Draft", "Pending approval", "In progress", "Sent", "Approved", "Paid", "Posted", "Partially paid", "Overdue", "Rejected", "Cancelled", "Archived", "SomethingUnknown", null].map((s) => (
              <StatusBadge key={String(s)} status={s} />
            ))}
          </div>
        </Section>

        <Section id="states" title="Empty, error, permission and loading states">
          <div className="grid gap-4 md:grid-cols-2">
            {(["empty", "no-results", "unavailable", "forbidden"] as const).map((variant) => (
              <div key={variant} className="card">
                <Label>{variant}</Label>
                <EmptyState variant={variant} compact action={variant === "empty" ? <button type="button" className="btn btn-sm bg-primary text-white">Add customer</button> : undefined} />
              </div>
            ))}
            <div className="card">
              <Label>ErrorState (500 with retry)</Label>
              <ErrorState error={demoError} onRetry={() => toast.info("Retry clicked")} compact />
            </div>
            <div className="card">
              <Label>PermissionDenied</Label>
              <PermissionDenied compact />
            </div>
            <div className="card p-4">
              <Label>SkeletonTable</Label>
              <SkeletonTable rows={3} columns={4} />
            </div>
            <div>
              <Label>SkeletonCard / SkeletonFields</Label>
              <SkeletonCard />
              <div className="card mt-3 p-4">
                <SkeletonFields fields={4} />
              </div>
            </div>
          </div>
        </Section>

        <Section id="kpi" title="KpiCard">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <KpiCard label="Revenue" value={4_215_300} format="money" currency="USD" previousValue={3_900_000} comparisonLabel="vs last month" trend={[3.1, 3.4, 3.2, 3.6, 3.9, 4.2]} to="/dev/components" icon="mgc_currency_dollar_line" />
            <KpiCard label="Overdue invoices" value={18} previousValue={12} higherIsBetter={false} comparisonLabel="vs last week" />
            <KpiCard label="Conversion rate" value={0.253} format="percent" previousValue={0.253} comparisonLabel="vs last quarter" />
            <KpiCard label="Loading" value={null} isLoading />
            <KpiCard label="Payroll status" value={null} unavailable />
            <KpiCard label="New from zero" value={5} previousValue={0} comparisonLabel="vs yesterday" />
          </div>
        </Section>

        <Section id="charts" title="ChartCard and WidgetBoundary">
          <div className="grid gap-4 md:grid-cols-2">
            <ChartCard
              title="Pipeline by stage"
              subtitle="Demo data · open opportunities"
              table={
                <table className="min-w-full text-sm">
                  <thead>
                    <tr>
                      <th scope="col" className="text-left">Stage</th>
                      <th scope="col" className="text-right">Count</th>
                    </tr>
                  </thead>
                  <tbody>
                    {[["Qualified", 12], ["Proposal", 8], ["Negotiation", 5]].map(([s, n]) => (
                      <tr key={s}>
                        <td>{s}</td>
                        <td className="text-right tabular-nums">{n}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              }
            >
              <ReactApexChart
                type="bar"
                height={220}
                series={[{ name: "Opportunities", data: [12, 8, 5] }]}
                options={{
                  chart: { toolbar: { show: false } },
                  colors: ["#3073F1"],
                  plotOptions: { bar: { borderRadius: 4, borderRadiusApplication: "end", columnWidth: "40%" } },
                  dataLabels: { enabled: false },
                  xaxis: { categories: ["Qualified", "Proposal", "Negotiation"] },
                  grid: { borderColor: "#e2e8f0" },
                }}
              />
            </ChartCard>
            <ChartCard title="Loading" isLoading>
              <div />
            </ChartCard>
            <ChartCard title="Empty" isEmpty emptyMessage="No invoices were issued in this period.">
              <div />
            </ChartCard>
            <ChartCard title="Error" error={demoError} onRetry={() => toast.info("Retry clicked")}>
              <div />
            </ChartCard>
            <ChartCard title="Unavailable" unavailable>
              <div />
            </ChartCard>
            <div>
              <button type="button" className="btn btn-sm mb-2 border border-slate-300" onClick={() => setBreakWidget((b) => !b)}>
                {breakWidget ? "Fix widget" : "Break widget"}
              </button>
              <WidgetBoundary name="Demo widget">
                {breakWidget ? <Thrower /> : <KpiCard label="Healthy widget" value={42} />}
              </WidgetBoundary>
            </div>
          </div>
        </Section>

        <Section id="table" title="DataTable, toolbar, filters, pagination">
          <div className="card">
            {selected.size > 0 ? (
              <BulkActionBar count={selected.size} onClear={() => setSelected(new Set())}>
                <button type="button" className="btn btn-sm border border-slate-300">Export selected</button>
              </BulkActionBar>
            ) : (
              <DataTableToolbar
                search={<SearchInput value={search} onSearch={setSearch} placeholder="Search demo customers…" />}
                filters={
                  <button type="button" className="btn border border-slate-300 dark:border-slate-600" onClick={() => setFiltersOpen(true)}>
                    <i className="mgc_filter_line me-1" aria-hidden="true" />
                    Filters
                  </button>
                }
                tools={<ColumnSelector columns={demoColumns} hidden={columnsState.hidden} onToggle={columnsState.toggle} onReset={columnsState.reset} />}
                onRefresh={() => toast.info("Refreshed")}
              />
            )}
            <FilterChips chips={[{ key: "status", label: "Status: Active" }]} onRemove={() => toast.info("Filter removed")} onClearAll={() => toast.info("Filters cleared")} />
            <DataTable<DemoRow>
              caption="Demo customers"
              columns={demoColumns}
              rows={demoRows}
              getRowId={(r) => r.id}
              hiddenColumns={columnsState.hidden}
              sort={sort}
              onSortChange={setSort}
              rowHref={() => "/dev/components#detail"}
              selectedIds={selected}
              onSelectionChange={setSelected}
              rowActions={[
                { label: "View", icon: "mgc_eye_line", onSelect: (r) => toast.info(`View ${r.code}`) },
                { label: "Edit", icon: "mgc_edit_line", onSelect: (r) => toast.info(`Edit ${r.code}`) },
                { label: "Delete", icon: "mgc_delete_2_line", tone: "danger", onSelect: () => setDeleteOpen("ok"), hidden: (r) => r.balance > 0 },
              ]}
              emptyState={<EmptyState />}
            />
            <Pagination pageNumber={page} pageSize={25} totalCount={612} onPageChange={setPage} onPageSizeChange={() => toast.info("Page size changed")} noun={["customer", "customers"]} />
          </div>
          <div className="mt-4 grid gap-4 md:grid-cols-3">
            <div className="card">
              <Label>Loading</Label>
              <DataTable<DemoRow> caption="Loading" columns={demoColumns} rows={undefined} isLoading getRowId={(r) => r.id} emptyState={null} />
            </div>
            <div className="card">
              <Label>Empty / no results</Label>
              <DataTable<DemoRow> caption="Empty" columns={demoColumns} rows={[]} isFiltered getRowId={(r) => r.id} emptyState={<EmptyState compact />} noResultsState={<EmptyState variant="no-results" compact />} />
            </div>
            <div className="card">
              <Label>First load failed</Label>
              <DataTable<DemoRow> caption="Error" columns={demoColumns} rows={undefined} error={demoError} onRetry={() => toast.info("Retry")} getRowId={(r) => r.id} emptyState={null} />
            </div>
          </div>
          <FilterPanel open={filtersOpen} onClose={() => setFiltersOpen(false)} onApply={() => setFiltersOpen(false)} onClear={() => toast.info("Cleared")}>
            <label className="block text-sm">
              Status
              <select className="form-select mt-1 w-full">
                <option>Any</option>
                <option>Active</option>
              </select>
            </label>
          </FilterPanel>
        </Section>

        <Section id="dialogs" title="Dialogs and toasts">
          <div className="flex flex-wrap gap-2">
            <button type="button" className="btn border border-slate-300" onClick={() => setConfirmOpen(true)}>ConfirmDialog</button>
            <button type="button" className="btn border border-danger text-danger" onClick={() => setDeleteOpen("ok")}>DeleteDialog (succeeds)</button>
            <button type="button" className="btn border border-danger text-danger" onClick={() => setDeleteOpen("conflict")}>DeleteDialog (409 conflict)</button>
            <button type="button" className="btn border border-slate-300" onClick={() => toast.success("Customer saved")}>Toast success</button>
            <button type="button" className="btn border border-slate-300" onClick={() => toast.error("Could not reach the server")}>Toast error</button>
          </div>
          <ConfirmDialog
            open={confirmOpen}
            title="Send quote to customer?"
            description="The customer will receive a PDF by email."
            confirmLabel="Send quote"
            onConfirm={() => {
              setConfirmOpen(false);
              toast.success("Quote sent");
            }}
            onCancel={() => setConfirmOpen(false)}
          />
          <DeleteDialog
            open={deleteOpen !== null}
            entityName="customer"
            recordLabel="Demo customer B (C-002)"
            requireTypedConfirmation={deleteOpen === "conflict"}
            onArchive={() => Promise.resolve()}
            onDelete={() =>
              deleteOpen === "conflict"
                ? Promise.reject(Object.assign(new Error("demo"), { response: { status: 409, data: { message: "It has 3 open sales orders." } } }))
                : Promise.resolve()
            }
            onDone={(action) => {
              setDeleteOpen(null);
              toast.success(`Customer ${action}`);
            }}
            onCancel={() => setDeleteOpen(null)}
          />
        </Section>

        <Section id="forms" title="Form fields, EntitySelect, LineItemsTable">
          <FormProvider {...methods}>
            <form
              className="space-y-4"
              onSubmit={methods.handleSubmit(() => {
                methods.setError("email", { type: "server", message: "Enter a valid email address." });
                setFormMessages(["Credit check service is unavailable (demo form-level error)."]);
              })}
            >
              <FormErrorSummary messages={formMessages} />
              <FormSection title="Basics" description="Required fields are marked with an asterisk.">
                <TextField name="name" label="Customer name" required placeholder="Acme Ltd" />
                <TextField name="email" label="Email" type="email" help="Invoices are sent here." />
                <SelectField name="type" label="Type" options={[{ value: "Individual", label: "Individual" }, { value: "Company", label: "Company" }]} />
                <EntitySelect
                  name="managerId"
                  label="Account manager"
                  loadOptions={async (q) => demoEntities.filter((e) => e.label.toLowerCase().includes(q.toLowerCase()))}
                  resolveOption={async (id) => demoEntities.find((e) => e.value === id) ?? null}
                />
                <CurrencyField name="creditLimit" label="Credit limit" currency="USD" />
                <NumberField name="quantity" label="Seats" min={0} />
                <DateField name="startDate" label="Customer since" />
                <CheckboxField name="isVip" label="VIP account" help="Shown on quotes and orders." />
                <FullWidth>
                  <TextareaField name="notes" label="Notes" rows={3} />
                </FullWidth>
              </FormSection>
              <FormSection title="Lines" columns={1}>
                <LineItemsTable name="lines" currency="USD" />
              </FormSection>
              <FormFooter onCancel={() => toast.info("Cancelled")} isSaving={false} submitLabel="Save (shows server errors)" />
            </form>
          </FormProvider>
        </Section>

        <Section id="detail" title="Detail page pieces">
          <DetailHeader
            title="Demo customer A"
            code="C-001"
            status="Active"
            facts={[
              { label: "Owner", value: "Demo employee Ayesha" },
              { label: "Credit limit", value: "$5,000.00" },
              { label: "Balance", value: "$1,250.00" },
              { label: "Customer since", value: "Sep 29, 2026" },
            ]}
            actions={<button type="button" className="btn bg-primary text-white">Edit</button>}
          />
          <DetailTabs
            tabs={[
              {
                key: "overview",
                label: "Overview",
                content: (
                  <FieldGrid
                    title="Contact"
                    fields={[
                      { label: "Email", value: "demo@example.test" },
                      { label: "Phone", value: null },
                      { label: "Address", value: "1 Demo Street, Lahore", wide: true },
                    ]}
                  />
                ),
              },
              {
                key: "orders",
                label: "Sales orders",
                count: 0,
                content: (
                  <RelatedRecordsPanel title="Sales orders" count={0} viewAllTo="/dev/components">
                    <EmptyState compact title="No sales orders" description="Orders for this customer will appear here." />
                  </RelatedRecordsPanel>
                ),
              },
              {
                key: "activity",
                label: "Activity",
                content: (
                  <ActivityTimeline
                    items={[
                      { id: "a2", at: "2026-09-28T10:15:00Z", actor: "Demo admin", title: "Status changed", changes: [{ field: "Status", before: "Pending", after: "Active" }] },
                      { id: "a1", at: "2026-09-27T08:00:00Z", actor: "Demo admin", title: "Created" },
                    ]}
                  />
                ),
              },
            ]}
          />
        </Section>
      </div>
    </div>
  );
}
