import { useState, type ReactNode } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { Menu, MenuButton, MenuItem, MenuItems } from "@headlessui/react";
import {
  ActivityTimeline,
  DataTable,
  DeleteDialog,
  DetailHeader,
  DetailTabs,
  EmptyState,
  ErrorState,
  FieldGrid,
  PageHeader,
  RelatedRecordsPanel,
  SkeletonCard,
  StatusBadge,
  toast,
  type Column,
} from "../../../../../components/erp";
import { formatDate, formatMoney, orDash } from "../../../../../lib/erp/format";
import { useResource, type ResourceState } from "../../../../../lib/erp/http";
import { usePermission } from "../../../../../lib/erp/permissions";
import { customerService } from "../services";
import type { CustomerDto } from "../types";
import { customerDisplayName, markCustomerInactive } from "../customerUtils";
import { contactService, type ContactDto } from "../../contact/services";
import { opportunityService, type OpportunityDto } from "../../opportunity/services";
import { quoteService, type QuoteDto } from "../../quote/services";
import { salesOrderService, type SalesOrderDto } from "../../sales-order/services";
import { activityService, type ActivityDto } from "../../activity/services";
import { coreApi, type AuditLogEntry } from "../../../../core/services/coreApi";
import { diffAuditEntry, REDACTED } from "../../../../core/utils/coreFormat";

export default function CustomerDetail() {
  const { id = "" } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const customer = useResource((customerId: string, signal) => customerService.get(customerId, signal), id);
  const canEdit = usePermission("crm.customers.update");
  const canDelete = usePermission("crm.customers.delete");
  const [deleting, setDeleting] = useState(false);

  // Related records load independently: one failing tab never breaks the page.
  const contacts = useResource((cid: string) => contactService.getByCustomer(cid), id);
  const opportunities = useResource((cid: string) => opportunityService.getByCustomer(cid), id);
  const quotes = useResource((cid: string) => quoteService.getByCustomer(cid), id);
  const orders = useResource((cid: string) => salesOrderService.getByCustomer(cid), id);
  const activities = useResource((cid: string) => activityService.getByCustomer(cid), id);
  const history = useResource((cid: string, signal) => coreApi.auditLogsForEntity("Customer", cid, signal), id);

  const breadcrumbs = [
    { label: "CRM", to: "/crm/dashboard" },
    { label: "Customers", to: "/crm/customers" },
    { label: customer.data?.customerName ?? "Customer" },
  ];

  if (customer.error) {
    return (
      <>
        <PageHeader title="Customer" breadcrumbs={breadcrumbs} />
        <div className="card">
          {customer.error.status === 404 ? (
            <EmptyState
              variant="no-results"
              title="Customer not found"
              description="It may have been deleted, or the link is out of date."
              action={
                <Link to="/crm/customers" className="btn border border-slate-300 dark:border-slate-600">
                  Back to customers
                </Link>
              }
            />
          ) : (
            <ErrorState error={customer.error} onRetry={customer.refetch} />
          )}
        </div>
      </>
    );
  }

  const c = customer.data;
  if (!c) {
    return (
      <>
        <PageHeader title="Customer" breadcrumbs={breadcrumbs} />
        <SkeletonCard lines={4} />
      </>
    );
  }

  return (
    <>
      <PageHeader title={c.customerName} breadcrumbs={breadcrumbs} />
      <DetailHeader
        title={c.customerName}
        code={c.customerCode}
        status={c.status}
        facts={[
          { label: "Type", value: c.customerType },
          { label: "Credit limit", value: formatMoney(c.creditLimit, c.currencyCode) },
          { label: "Outstanding balance", value: formatMoney(c.outstandingBalance, c.currencyCode) },
          { label: "Customer since", value: formatDate(c.createdAt) },
        ]}
        actions={
          canEdit && (
            <Link to={`/crm/customers/${c.id}/edit`} className="btn bg-primary text-white">
              <i className="mgc_edit_line me-1" aria-hidden="true" />
              Edit
            </Link>
          )
        }
        more={
          canDelete && (
            <Menu>
              <MenuButton className="btn border border-slate-300 dark:border-slate-600" aria-label="More actions">
                <i className="mgc_more_2_fill" aria-hidden="true" />
              </MenuButton>
              <MenuItems anchor="bottom end" className="z-50 min-w-40 rounded-md border border-slate-200 bg-white py-1 shadow-lg focus:outline-none dark:border-slate-700 dark:bg-gray-800">
                <MenuItem>
                  <button type="button" onClick={() => setDeleting(true)} className="flex w-full items-center gap-2 px-3 py-1.5 text-left text-sm text-danger data-[focus]:bg-slate-100 dark:data-[focus]:bg-slate-700">
                    <i className="mgc_delete_2_line" aria-hidden="true" />
                    Delete customer
                  </button>
                </MenuItem>
              </MenuItems>
            </Menu>
          )
        }
      />

      <DetailTabs
        tabs={[
          { key: "overview", label: "Overview", content: <Overview customer={c} /> },
          {
            key: "contacts",
            label: "Contacts",
            count: contacts.data?.length,
            content: (
              <Related
                title="Contacts"
                state={contacts}
                columns={contactColumns}
                getRowId={(r) => r.id ?? r.email ?? r.fullName}
                rowHref={(r) => `/crm/contacts/${r.id}/edit`}
                addTo={`/crm/contacts/new?customerId=${c.id}`}
                addLabel="Add contact"
              />
            ),
          },
          {
            key: "opportunities",
            label: "Opportunities",
            count: opportunities.data?.length,
            content: <Related title="Opportunities" state={opportunities} columns={opportunityColumns} getRowId={(r) => r.id ?? ""} rowHref={(r) => `/crm/opportunities/${r.id}/edit`} />,
          },
          {
            key: "quotes",
            label: "Quotes",
            count: quotes.data?.length,
            content: <Related title="Quotes" state={quotes} columns={quoteColumns} getRowId={(r) => r.id ?? ""} rowHref={(r) => `/crm/quotes/${r.id}/edit`} />,
          },
          {
            key: "orders",
            label: "Sales orders",
            count: orders.data?.length,
            content: <Related title="Sales orders" state={orders} columns={orderColumns} getRowId={(r) => r.id ?? ""} rowHref={(r) => `/crm/sales-orders/${r.id}/edit`} />,
          },
          {
            key: "activities",
            label: "Activities",
            count: activities.data?.length,
            content: <Related title="Activities" state={activities} columns={activityColumns} getRowId={(r) => r.id ?? ""} rowHref={(r) => `/crm/activities/${r.id}/edit`} />,
          },
          {
            key: "history",
            label: "History",
            count: history.data?.length,
            content: <CustomerHistory state={history} />,
          },
        ]}
      />

      <DeleteDialog
        open={deleting}
        entityName="customer"
        recordLabel={customerDisplayName(c)}
        onDelete={() => customerService.remove(c.id!)}
        onArchive={c.status !== "Inactive" && canEdit ? () => markCustomerInactive(c) : undefined}
        onDone={(action) => {
          setDeleting(false);
          if (action === "deleted") {
            toast.success("Customer deleted");
            navigate("/crm/customers", { replace: true });
          } else {
            toast.success("Customer marked inactive");
            customer.refetch();
          }
        }}
        onCancel={() => setDeleting(false)}
      />
    </>
  );
}

function Overview({ customer: c }: { customer: CustomerDto }) {
  const address = [c.address, [c.city, c.state, c.postalCode].filter(Boolean).join(", "), c.country].filter(Boolean).join("\n");
  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <FieldGrid
        title="Contact"
        fields={[
          { label: "Primary contact", value: c.contactPerson },
          { label: "Email", value: c.email && <a className="text-primary hover:underline" href={`mailto:${c.email}`}>{c.email}</a> },
          { label: "Phone", value: c.phone && <a className="text-primary hover:underline" href={`tel:${c.phone}`}>{c.phone}</a> },
          { label: "Website", value: c.website && <a className="text-primary hover:underline" href={c.website} target="_blank" rel="noreferrer noopener">{c.website}</a> },
          { label: "Address", value: address && <span className="whitespace-pre-line">{address}</span>, wide: true },
        ]}
      />
      <FieldGrid
        title="Billing"
        fields={[
          { label: "Currency", value: c.currencyCode },
          { label: "Payment terms", value: c.paymentTerms },
          { label: "Credit limit", value: formatMoney(c.creditLimit, c.currencyCode) },
          { label: "Outstanding balance", value: formatMoney(c.outstandingBalance, c.currencyCode) },
          { label: "Tax ID", value: c.taxId },
          { label: "Industry", value: c.industry },
          { label: "Company", value: c.companyName !== c.customerName ? c.companyName : null },
        ]}
      />
      {c.notes && (
        <div className="lg:col-span-2">
          <FieldGrid title="Notes" fields={[{ label: "Internal notes", value: <span className="whitespace-pre-line">{c.notes}</span>, wide: true }]} />
        </div>
      )}
    </div>
  );
}

function Related<T>({
  title,
  state,
  columns,
  getRowId,
  rowHref,
  addTo,
  addLabel,
}: {
  title: string;
  state: ResourceState<T[]>;
  columns: Column<T>[];
  getRowId: (row: T) => string;
  rowHref: (row: T) => string;
  addTo?: string;
  addLabel?: string;
}) {
  const action: ReactNode = addTo && (
    <Link to={addTo} className="text-primary hover:underline">
      {addLabel}
    </Link>
  );
  return (
    <RelatedRecordsPanel title={title} count={state.data?.length} action={action}>
      <DataTable<T>
        caption={title}
        columns={columns}
        rows={state.data}
        getRowId={getRowId}
        rowHref={rowHref}
        isLoading={state.isLoading}
        error={state.error}
        onRetry={state.refetch}
        emptyState={<EmptyState compact title={`No ${title.toLowerCase()}`} description={`${title} for this customer will appear here.`} />}
      />
    </RelatedRecordsPanel>
  );
}

const humanizeField = (field: string) => field.replace(/([a-z])([A-Z])/g, "$1 $2").replace(/^./, (c) => c.toUpperCase());
const showValue = (v: unknown) => (v === undefined || v === null || v === "" ? undefined : v === REDACTED ? "hidden" : String(v));

/** The customer's audit trail (written automatically since Checkpoint 1). */
function CustomerHistory({ state }: { state: ResourceState<AuditLogEntry[]> }) {
  if (state.error) return <div className="card"><ErrorState error={state.error} onRetry={state.refetch} compact /></div>;
  if (!state.data) return <SkeletonCard lines={4} />;
  return (
    <div className="card p-5">
      <ActivityTimeline
        emptyText="Changes to this customer are recorded from now on."
        items={state.data.map((entry) => ({
          id: entry.id,
          at: entry.timestamp,
          actor: entry.userName ?? undefined,
          title: entry.action,
          changes:
            entry.action === "Updated"
              ? diffAuditEntry(entry).map((c) => ({ field: humanizeField(c.field), before: showValue(c.before), after: showValue(c.after) }))
              : undefined,
        }))}
      />
    </div>
  );
}

const money = (amount: number | undefined, currency: string | undefined) => <span className="tabular-nums">{formatMoney(amount, currency)}</span>;

const contactColumns: Column<ContactDto>[] = [
  { key: "name", header: "Name", cell: (r) => r.fullName || `${r.firstName} ${r.lastName}` },
  { key: "jobTitle", header: "Job title", cell: (r) => orDash(r.jobTitle) },
  { key: "email", header: "Email", cell: (r) => orDash(r.email) },
  { key: "phone", header: "Phone", cell: (r) => orDash(r.phone) },
];

const opportunityColumns: Column<OpportunityDto>[] = [
  { key: "name", header: "Opportunity", cell: (r) => r.opportunityName || r.title || r.opportunityCode || "Untitled" },
  { key: "stage", header: "Stage", cell: (r) => <StatusBadge status={r.stage} /> },
  { key: "value", header: "Value", align: "right", cell: (r) => money(r.estimatedValue ?? r.amount, r.currency) },
  { key: "probability", header: "Probability", align: "right", cell: (r) => (r.probability !== undefined ? `${r.probability}%` : "—") },
  { key: "close", header: "Expected close", cell: (r) => formatDate(r.expectedCloseDate) },
];

const quoteColumns: Column<QuoteDto>[] = [
  { key: "number", header: "Quote", cell: (r) => r.quoteNumber ?? "Draft" },
  { key: "date", header: "Date", cell: (r) => formatDate(r.quoteDate) },
  { key: "validUntil", header: "Valid until", cell: (r) => formatDate(r.validUntil) },
  { key: "status", header: "Status", cell: (r) => <StatusBadge status={r.status} /> },
  { key: "total", header: "Total", align: "right", cell: (r) => money(r.totalAmount, r.currency) },
];

const orderColumns: Column<SalesOrderDto>[] = [
  { key: "number", header: "Order", cell: (r) => r.salesOrderNumber || r.orderNumber },
  { key: "date", header: "Order date", cell: (r) => formatDate(r.orderDate) },
  { key: "status", header: "Status", cell: (r) => <StatusBadge status={r.status} /> },
  { key: "total", header: "Total", align: "right", cell: (r) => money(r.totalAmount, r.currency) },
];

const activityColumns: Column<ActivityDto>[] = [
  { key: "subject", header: "Subject", cell: (r) => r.subject },
  { key: "type", header: "Type", cell: (r) => r.activityType || r.type },
  { key: "date", header: "Date", cell: (r) => formatDate(r.startDate) },
  { key: "status", header: "Status", cell: (r) => <StatusBadge status={r.status} /> },
  { key: "owner", header: "Assigned to", cell: (r) => orDash(r.assignedToUserName ?? r.assignedTo) },
];
